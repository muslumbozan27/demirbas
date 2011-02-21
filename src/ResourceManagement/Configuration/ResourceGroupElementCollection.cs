
using System;
using System.Collections.Generic;
using System.Configuration;

namespace AlmWitt.Web.ResourceManagement.Configuration
{
	/// <summary>
	/// Represents a collection of <see cref="ResourceGroupElement"/>'s.
	/// </summary>
	/// <typeparam name="TGroupElement"></typeparam>
	public class ResourceGroupElementCollection<TGroupElement> : ConfigurationElementCollection, IEnumerable<ResourceGroupElement>
		where TGroupElement : ResourceGroupElement, new()
	{
		/// <summary>
		/// Gets a <see cref="ResourceGroupElement"/> by index number.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public TGroupElement this[int index]
		{
			get
			{
				return (TGroupElement) BaseGet(index);
			}
		}

		/// <summary>
		/// Gets or sets whether consolidation is enabled for this type of resource.
		/// </summary>
		[ConfigurationProperty(PropertyNames.Consolidate, IsRequired = false, DefaultValue = true)]
		public bool Consolidate
		{
			get { return (bool)this[PropertyNames.Consolidate]; }
			set { this[PropertyNames.Consolidate] = value; }
		}

		/// <summary>
		/// Gets or sets whether the scripts will be compressed when they are consolidated.
		/// </summary>
		[ConfigurationProperty(PropertyNames.Compress, DefaultValue = false)]
		public bool Compress
		{
			get { return (bool)this[PropertyNames.Compress]; }
			set
			{
				this[PropertyNames.Compress] = value;
				foreach (ResourceGroupElement groupElement in this)
				{
					groupElement.CompressDefaultValue = value;
				}
			}
		}

		/// <summary>
		/// Adds the given <see cref="ResourceGroupElement"/> to the collection.
		/// </summary>
		/// <param name="element"></param>
		public void Add(TGroupElement element)
		{
			BaseAdd(element);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			var element = new TGroupElement();
			//set the compress property so that it defaults to the value
			//set by its parent (i.e. this collection).
			element.CompressDefaultValue = Compress;

			return element;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ResourceGroupElement) element).ConsolidatedUrl;
		}

		#region IEnumerable<ResourceGroupElement> implimentation

		IEnumerator<ResourceGroupElement> IEnumerable<ResourceGroupElement>.GetEnumerator()
		{
			foreach (ResourceGroupElement element in this)
			{
				yield return element;
			}
		}

		#endregion
	}

	internal delegate void ResourceGroupElementProcessor<TGroupElement>(TGroupElement groupElement, IResourceFilter excludeFilter)
		where TGroupElement : ResourceGroupElement, new();
}
