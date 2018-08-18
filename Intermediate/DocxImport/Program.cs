﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using NGS.Templater;

namespace DocxImport
{
	public class Program
	{
		public static void Main(string[] args)
		{
			File.Copy("template/Master.docx", "Master.docx", true);
			var docx = ExtractDocumentBody();
			var elements =
				(from p in docx.Descendants(XName.Get("p", "http://schemas.openxmlformats.org/wordprocessingml/2006/main"))
				 where !HasIdReference(p)
				 select p).ToArray();
			using (var doc = Configuration.Factory.Open("Master.docx"))
				doc.Templater.Replace("imported_document", elements);
			Process.Start("Master.docx");
		}

		private static readonly XName RID = XName.Get("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		private static readonly XName WID = XName.Get("id", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
		private static readonly XName Embed = XName.Get("embed", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

		private static bool HasIdReference(XElement node)
		{
			if (node.Attribute(RID) != null
				|| node.Attribute(WID) != null
				|| node.Attribute(Embed) != null) return true;
			foreach (var child in node.Descendants())
			{
				if (HasIdReference(child))
					return true;
			}
			return false;
		}

		private static XElement ExtractDocumentBody()
		{
			using (var stream = File.Open("template/ToImport.docx", FileMode.Open))
			{
				var package = ZipPackage.Open(stream, FileMode.OpenOrCreate);
				return XElement.Load(package.GetPart(new Uri("/word/document.xml", UriKind.Relative)).GetStream());
			}
		}
	}
}
