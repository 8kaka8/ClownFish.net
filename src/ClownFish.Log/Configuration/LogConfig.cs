﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ClownFish.Base.Xml;

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
		/// 所有Writer的配置集合
		/// </summary>
		public WritersConfig Writers { get; set; }

		/// <summary>
		/// 所有要写入的数据类型集合
		/// </summary>
		[XmlArrayItem("Type")]
		public List<TypeItemConfig> Types { get; set; }

		/// <summary>
		/// 性能日志的配置信息
		/// </summary>
		public PerformanceConfig Performance { get; set; }


		/// <summary>
		/// 写日志失败时，异常消息的写入器
		/// </summary>
		public string ExceptionWriter { get; set; }


		/// <summary>
		/// 加载默认的配置文件（ClownFish.Log.config）
		/// </summary>
		/// <returns></returns>
		public static LogConfig ReadConfigFile()
		{
			string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClownFish.Log.config");
			if( File.Exists(configFile) == false )
				throw new FileNotFoundException("配置文件不存在：" + configFile);

			return XmlHelper.XmlDeserializeFromFile<LogConfig>(configFile);
		}

	}


	


	

	


	

	



}
