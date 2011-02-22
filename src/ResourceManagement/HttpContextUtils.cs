using System;
using System.Collections;

namespace AlmWitt.Web.ResourceManagement
{
	internal static class HttpContextUtils
	{
		public static T GetOrCreate<T>(this IDictionary httpItems, object key)
			where T : class, new()
		{
			return httpItems.GetOrCreate<T>(key, () => new T());
		}

		public static T GetOrCreate<T>(this IDictionary httpItems, object key, Func<T> create)
			where T : class
		{
			var list = httpItems[key] as T;
			if (list != null)
				return list;

			list = create();
			httpItems[key] = list;

			return list;
		}
	}
}