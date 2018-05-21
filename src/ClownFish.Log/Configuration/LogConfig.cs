﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ClownFish.Base;
using ClownFish.Base.Xml;
using ClownFish.Log.Serializer;

namespace ClownFish.Log.Configuration
{
	/// <summary>
	/// 日志的配置数据结构
	/// </summary>
	public sealed class LogConfig
	{
		/// <summary>
		/// 是否启用
		/// </summary>
		public bool Enable { get; set; }

		/// <summary>
		/// 定时刷新间隔
		/// </summary>
		public int TimerPeriod { get; set; }

        /// <summary>
		/// 性能日志的配置信息
		/// </summary>
		public PerformanceConfig Performance { get; set; }


        /// <summary>
        /// 写日志失败时，异常消息的写入器
        /// </summary>
        public string ExceptionWriter { get; set; }

        /// <summary>
        /// 异常日志中是否包含环境信息。
        /// 老版本的异常日志是不包含环境信息的。
        /// 使用建议：如果环境可控，例如Web站点，可以不开启；
        /// 如果是给客户私有部署，环境各不相同，当出现异常时想参考客户运行环境，那么可以开启此参数。
        /// </summary>
        public bool LogEnvironmentInfo { get; set; }


        /// <summary>
        /// 所有Writer的配置集合
        /// </summary>
        [XmlArrayItem("Writer")]
		public WriterSection[] Writers { get; set; }

		/// <summary>
		/// 所有要写入的数据类型集合
		/// </summary>
		[XmlArrayItem("Type")]
		public TypeItemConfig[] Types { get; set; }

		


        /// <summary>
        /// 加载默认的配置文件（ClownFish.Log.config）
        /// </summary>
        /// <returns></returns>
        public static LogConfig ReadConfigFile()
		{
			string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClownFish.Log.config");
			if( RetryFile.Exists(configFile) == false )
				throw new FileNotFoundException("配置文件不存在：" + configFile);

			return XmlHelper.XmlDeserializeFromFile<LogConfig>(configFile);
		}

		/// <summary>
		/// 获取当前正在使用的配置对象
		/// </summary>
		/// <returns></returns>
		public static LogConfig GetCurrent()
		{
			return WriterFactory.Config;
		}

	}


	


	

	


	

	



}
