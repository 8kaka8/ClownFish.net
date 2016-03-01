﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Reflection;

namespace ClownFish.MockAspnetRuntime
{
	public class MockHttpApplication
	{
		private HttpApplication _application;

		public HttpApplication Instance
		{
			get { return _application; }
		}


		internal MockHttpApplication(HttpContext context)
        {
			_application = new HttpApplication();

			_application.GetType().InvokeMember("_context",
				BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic,
				null, _application,
				new object[] { context });			
        }

	}
}
