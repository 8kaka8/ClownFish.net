﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ClownFish.Base.Reflection;

namespace ClownFish.Base.Http
{
    /// <summary>
    /// HTTP头的存储集合
    /// </summary>
    public sealed class HttpHeaderCollection : List<NameValue>
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public HttpHeaderCollection() : base(8) { }


        /// <summary>
        /// 将一个字典转换成HttpHeaderCollection实例
        /// </summary>
        /// <param name="dictionary"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
        public static implicit operator HttpHeaderCollection(Dictionary<string, string> dictionary)
        {
            // 说明：
            // 1、这个方法不能定义成支持匿名对象，因为编译器不让通过
            // 2、请求头的KEY名经常包含横线，这个也是匿名对象不能支持的

            if( dictionary == null )
                throw new ArgumentNullException(nameof(dictionary));


            HttpHeaderCollection result = new HttpHeaderCollection();

            foreach( var kv in dictionary )
                result.Add(kv.Key, kv.Value);

            return result;
        }


        /// <summary>
        /// 将一个匿名对象转换成HttpHeaderCollection实例
        /// </summary>
        /// <param name="obj"></param>
        public static HttpHeaderCollection create(object obj)
        {
            // 说明： C# 编译器不允许 从 object 到 HttpHeaderCollection 的类型转换，所以只能这样定义一个方法。

            // 对参数做个简单的判断，防止传入了错误的类型
            if( obj.GetType().IsPrimitive || obj.GetType() == typeof(string) )
                throw new ArgumentException("参数类型不正确，应该传递一个匿名对象。");


            HttpHeaderCollection result = new HttpHeaderCollection();

            var ps = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(var p in ps ) {
                string name = p.Name.Replace('_', '-');
                string value = (p.FastGetValue(obj) ?? string.Empty).ToString();
                result.Add(name, value);
            }

            return result;
        }



        /// <summary>
        /// 索引器，根据名称访问集合
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string this[string name] {
            get {
                // 如果KEY重复，这里只返回第一个匹配的结果
                // KEY重复的场景需要提供GetValues方法，暂且先不实现
                NameValue item = this.FirstOrDefault(
                    x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                if( item != null )
                    return item.Value;
                else
                    return null;
            }
            set {
                NameValue item = this.FirstOrDefault(
                    x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                if( item != null )
                    item.Value = value;
                else {
                    NameValue nv = new NameValue { Name = name, Value = value };
                    this.Add(nv);
                }
            }
        }


        /// <summary>
        /// 增加一个键值对
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            if( string.IsNullOrEmpty(name) )
                throw new ArgumentNullException("name");

            //if( string.IsNullOrEmpty(value) )
            if( value == null )
                throw new ArgumentNullException("value");

            NameValue nv = new NameValue { Name = name, Value = value };
            this.Add(nv);
        }

        /// <summary>
        /// 根据指定的名称删除键值列表元素
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string name)
        {
            if( string.IsNullOrEmpty(name) )
                throw new ArgumentNullException("name");


            int index = this.FindIndex(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if( index >= 0 )
                this.RemoveAt(index);
        }



    }
}
