﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClownFish.Base.Common;
using ClownFish.Base.Reflection;
using ClownFish.Base.TypeExtend;
using ClownFish.Web.Reflection;

namespace ClownFish.Web.Serializer
{
	/// <summary>
	/// 所有Action参数提供者的基类，提供允许从HTTP请求中构造参数的功能
	/// </summary>
	public abstract class BaseDataProvider
	{
		static BaseDataProvider()
		{
			RegisterHttpDataConvert<HttpFile>(HttpFile.GetFromHttpRequest);
			RegisterHttpDataConvert<HttpFile[]>(HttpFile.GetFilesFromHttpRequest);
		}

		private static readonly Hashtable s_convertTable = Hashtable.Synchronized(new Hashtable(256));


		[MethodImpl(MethodImplOptions.Synchronized)]
		internal static void RegisterHttpDataConvert<T>(Func<HttpContext, ParameterInfo, T> func)
		{
			// 构造一个弱类型的委托，供后续使用。
			Func<HttpContext, ParameterInfo, object> convert = (HttpContext context, ParameterInfo p) => (object)func(context, p);

			// 采用覆盖的方式，如果类型注册多次，以最后一次为准
			s_convertTable[typeof(T)] = convert;
		}


		/// <summary>
		/// 根据指定的参数信息，从HTTP请求中构造参数
		/// </summary>
		/// <param name="context"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		protected object GetParameterFromHttp(HttpContext context, ParameterInfo p)
		{
			object value = null;

			if( TryGetSpecialParameter(context, p, out value) )
				return value;
			else
				return GetObjectFromHttp(context, p);
		}


		/// <summary>
		/// 根据指定的参数信息，尝试从HTTP上下文中获取参数值
		/// </summary>
		/// <param name="context">HttpContext实例</param>
		/// <param name="p">ParameterInfo实例</param>
		/// <param name="value">获取到的参数值（如何方法 return true;）</param>
		/// <returns>如果解析成功（确实存在特殊参数），返回 true，否则返回 false。如果是false，子类需要继续解析参数值</returns>
		private bool TryGetSpecialParameter(HttpContext context, ParameterInfo p, out object value)
		{
			value = null;

			if( p.IsOut )
				throw new NotImplementedException();

			if( p.ParameterType == typeof(VoidType) )
				return true;		// 忽略用于方法重载识别的【空参数】


			if( p.ParameterType == typeof(HttpContext) ) {
				value = context;
			}
			else if( p.ParameterType == typeof(NameValueCollection) ) {
				if( string.Compare(p.Name, "Form", StringComparison.OrdinalIgnoreCase) == 0 )
					value = context.Request.Form;
				else if( string.Compare(p.Name, "QueryString", StringComparison.OrdinalIgnoreCase) == 0 )
					value = context.Request.QueryString;
				else if( string.Compare(p.Name, "Headers", StringComparison.OrdinalIgnoreCase) == 0 )
					value = context.Request.Headers;
				else if( string.Compare(p.Name, "ServerVariables", StringComparison.OrdinalIgnoreCase) == 0 )
					value = context.Request.ServerVariables;
			}
			else {
				ContextDataAttribute[] rdAttrs = (ContextDataAttribute[])p.GetCustomAttributes(typeof(ContextDataAttribute), false);
				if( rdAttrs.Length == 1 )
					value = EvalFromHttpContext(context, rdAttrs[0], p);
				else
					// 需要子类解析的参数
					return false;
			}


			return true;
		}



		private object EvalFromHttpContext(HttpContext context, ContextDataAttribute attr, ParameterInfo p)
		{
			// 直接从HttpRequest对象中获取数据，根据Attribute中指定的表达式求值。
			string expression = attr.Expression;
			object requestData = null;

			if( expression.StartsWith("Request.") )
				requestData = System.Web.UI.DataBinder.Eval(context.Request, expression.Substring(8));

			else if( expression.StartsWith("HttpRuntime.") ) {
				PropertyInfo property = typeof(HttpRuntime).GetProperty(expression.Substring(12), BindingFlags.Static | BindingFlags.Public);
				if( property == null )
					throw new ArgumentException(string.Format("参数 {0} 对应的ContextDataAttribute计算表达式 {1} 无效：", p.Name, expression));
				requestData = property.FastGetValue(null);
			}
			else
				requestData = System.Web.UI.DataBinder.Eval(context, expression);


			if( requestData == null )
				return null;
			else {
				if( requestData.GetType().IsCompatible(p.ParameterType) )
					return requestData;
				else
					throw new ArgumentException(string.Format("参数 {0} 的申明的类型与HttpRequest对应属性的类型不一致。", p.Name));
			}
		}


		/// <summary>
		/// 根据指定的参数信息，从HTTP请求中构造【对象类型】参数
		/// </summary>
		/// <param name="context"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		private object GetObjectFromHttp(HttpContext context, ParameterInfo p)
		{
			Type paramterType = p.ParameterType.GetRealType();
			LazyObject<ModelBuilder> builder = new LazyObject<ModelBuilder>();

			// 如果参数是可支持的类型，则直接从HttpRequest中读取并赋值
			if( paramterType.IsSupportableType() ) {
				object val = builder.Instance.GetValueFromHttp(context, p.Name, paramterType, null);
				if( val != null )
					return val;
				else {
					if( p.ParameterType.IsValueType && p.ParameterType.IsNullableType() == false )
						throw new ArgumentException("未能找到指定的参数值：" + p.Name);
					else
						return null;
				}
			}


			// 检查是否存在自定义的类型转换委托
			Func<HttpContext, ParameterInfo, object> func = s_convertTable[paramterType] as Func<HttpContext, ParameterInfo, object>;
			if( func != null ) {
				// 使用自定义的类型转换委托
				return func(context, p);
			}


			// 自定义的类型。首先创建实例，然后给所有成员赋值。
			// 注意：这里不支持嵌套类型的自定义类型。	
			return builder.Instance.CreateObjectFromHttp(context, p);
		}

		


	}
}
