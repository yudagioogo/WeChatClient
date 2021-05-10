using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace GX
{
	/// <summary>
	/// 代码生成器，仿照T4的生成方式
	/// </summary>
	public class CodeWriter
	{
		public CodeWriter()
		{
			CurrentIndent = string.Empty;
			NewLineChars = Environment.NewLine;
			IndentChars = "\t";
		}

		/// <summary>
		/// 获取或设置要用于分行符的字符串。默认为<c>Environment.NewLine</c>
		/// </summary>
		public string NewLineChars { get; set; }
		private bool endsWithNewline;

		private readonly List<object> list = new List<object>();
		private StringBuilder generationEnvironmentField;

		/// <summary>
		/// The string builder that generation-time code is using to assemble generated output
		/// </summary>
		protected StringBuilder GenerationEnvironment
		{
			get
			{
				if ((this.generationEnvironmentField == null))
				{
					this.GenerationEnvironment = new StringBuilder();
				}
				return this.generationEnvironmentField;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				this.list.Add(value);
				this.generationEnvironmentField = value;
			}
		}

		/// <summary>
		/// 插入另外一个代码片段，方便分段生成
		/// </summary>
		/// <param name="segment"></param>
		public void AddSegment(CodeWriter segment)
		{
			if(segment == this)
				throw new ArgumentException("Can't add segment into itself.");
			this.list.Add(segment);
			this.GenerationEnvironment = new StringBuilder();
		}

		#region Write
		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		public CodeWriter Write(string textToAppend)
		{
			if (string.IsNullOrEmpty(textToAppend))
			{
				return this;
			}
			// If we're starting off, or if the previous text ended with a newline,
			// we have to append the current indent first.
			if (this.GenerationEnvironment.Length == 0 || this.endsWithNewline)
			{
				this.GenerationEnvironment.Append(this.CurrentIndent);
				this.endsWithNewline = false;
			}
			// Check if the current text ends with a newline
			if (textToAppend.EndsWith(NewLineChars, StringComparison.CurrentCulture))
			{
				this.endsWithNewline = true;
			}
			// This is an optimization. If the current indent is "", then we don't have to do any
			// of the more complex stuff further down.
			if (this.CurrentIndent.Length == 0)
			{
				this.GenerationEnvironment.Append(textToAppend);
				return this;
			}
			// Everywhere there is a newline in the text, add an indent after it
			textToAppend = textToAppend.Replace(NewLineChars, (NewLineChars + this.CurrentIndent));
			// If the text ends with a newline, then we should strip off the indent added at the very end
			// because the appropriate indent will be added when the next time Write() is called
			if (this.endsWithNewline)
			{
				this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.CurrentIndent.Length));
			}
			else
			{
				this.GenerationEnvironment.Append(textToAppend);
			}
			return this;
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		public CodeWriter Write(string format, params object[] args)
		{
			return this.Write(string.Format(CultureInfo.CurrentCulture, format, args));
		}

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		public CodeWriter WriteLine(string textToAppend)
		{
			this.Write(textToAppend);
			this.GenerationEnvironment.AppendLine();
			this.endsWithNewline = true;
			return this;
		}

		public CodeWriter WriteLine()
		{
			this.GenerationEnvironment.AppendLine();
			this.endsWithNewline = true;
			return this;
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		public CodeWriter WriteLine(string format, params object[] args)
		{
			return this.WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
		}

		public CodeWriter WriteIf(bool condition, string textToAppend)
		{
			return condition
				? this.Write(textToAppend)
				: this;
		}

		public CodeWriter WriteIf(bool condition, string format, params object[] args)
		{
			return condition
				? this.Write(format, args)
				: this;
		}

		public CodeWriter WriteLineIf(bool condition)
		{
			return condition
				? this.WriteLine()
				: this;
		}

		public CodeWriter WriteLineIf(bool condition, string textToAppend)
		{
			return condition
				? this.WriteLine(textToAppend)
				: this;
		}

		public CodeWriter WriteLineIf(bool condition, string format, params object[] args)
		{
			return condition
				? this.WriteLine(format, args)
				: this;
		}
		#endregion

		#region Indent
		private List<int> indentLengthsField;

		/// <summary>
		/// A list of the lengths of each indent that was added with PushIndent
		/// </summary>
		private List<int> indentLengths
		{
			get
			{
				if ((this.indentLengthsField == null))
				{
					this.indentLengthsField = new List<int>();
				}
				return this.indentLengthsField;
			}
		}

		/// <summary>
		/// Gets the current indent we use when adding lines to the output
		/// </summary>
		public string CurrentIndent { get; private set; }

		/// <summary>
		/// Increase the indent
		/// </summary>
		public CodeWriter PushIndent(string indent)
		{
			if ((indent == null))
			{
				throw new ArgumentNullException("indent");
			}
			this.CurrentIndent = (this.CurrentIndent + indent);
			this.indentLengths.Add(indent.Length);
			return this;
		}
		/// <summary>
		/// Remove the last indent that was added with PushIndent
		/// </summary>
		public CodeWriter PopIndent()
		{
			if (this.indentLengths.Count > 0)
			{
				int indentLength = this.indentLengths[this.indentLengths.Count - 1];
				this.indentLengths.RemoveAt(this.indentLengths.Count - 1);
				if (indentLength > 0)
				{
					this.CurrentIndent.Substring(this.CurrentIndent.Length - indentLength);
					this.CurrentIndent = this.CurrentIndent.Remove(this.CurrentIndent.Length - indentLength);
				}
			}
			return this;
		}

		/// <summary>
		/// Remove any indentation
		/// </summary>
		public CodeWriter ClearIndent()
		{
			this.indentLengths.Clear();
			this.CurrentIndent = string.Empty;
			return this;
		}

		/// <summary>
		/// 获取或设置缩进时默认要使用的字符串。默认为<c>'\t'</c>
		/// </summary>
		public string IndentChars { get; set; }

		public CodeWriter PushIndent()
		{
			return this.PushIndent(IndentChars);
		}
		public CodeWriter PushIndent(int level)
		{
			for (var i = 0; i < level; i++ )
				this.PushIndent(IndentChars);
			return this;
		}
		public CodeWriter PopIndent(int level)
		{
			Enumerable.Repeat(this.PopIndent(), level);
			return this;
		}
		#endregion

		public override string ToString()
		{
			return string.Concat(list);
		}
	}
}
