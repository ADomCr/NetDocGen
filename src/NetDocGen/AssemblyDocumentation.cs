﻿using NetDocGen.Xml;
using System.Reflection;

namespace NetDocGen
{
	public class AssemblyDocumentation : CommonDocumentation
	{
		public override string Name { get; }

		public override string FullName { get { return this.Name; } }

		public IEnumerable<NamespaceDocumentation> Namespaces
		{
			get
			{
				return this._namespaces.Values;
			}
		}

		private Assembly _assembly;

		private readonly Dictionary<string, NamespaceDocumentation> _namespaces = new();

		public AssemblyDocumentation(string name)
		{
			this.Name = name;
		}

		public AssemblyDocumentation(Assembly assembly)
		{
			this._assembly = assembly;

			this.Name = Path.GetFileNameWithoutExtension(assembly.Location);
			this.processAssembly();
		}

		public override AssemblyDocumentation GetRoot()
		{
			return this;
		}

		public void AddNamespace(NamespaceDocumentation ns)
		{
			this._namespaces.Add(ns.FullName, ns);
		}

		public NamespaceDocumentation GetNamespace(string name)
		{
			return this._namespaces[name];
		}

		public bool TryGetNamespace(string name, out NamespaceDocumentation ns)
		{
			return this._namespaces.TryGetValue((string)name, out ns);
		}

		public void UpdateComments(string path)
		{
			using (XmlParser parser = new XmlParser(path))
			{
				parser.ParseAssembly(this);
			}
		}

		private void processAssembly()
		{
			foreach (Type t in this._assembly.ExportedTypes)
			{
				if (t.IsNested)
				{
					continue;
				}

				if (!this._namespaces.TryGetValue(t.Namespace, out NamespaceDocumentation ns))
				{
					ns = new NamespaceDocumentation(t.Namespace, this);
					this._namespaces.Add(t.Namespace, ns);
				}

				TypeDocumentation tdoc = new(t, ns);
				ns.Types.Add(tdoc);
			}
		}
	}
}