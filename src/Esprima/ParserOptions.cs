using Esprima.Ast;

namespace Esprima
{
    /// <summary>
    /// Parser options.
    /// </summary>
    public class ParserOptions
    {
        /// <summary>
        /// Create a new <see cref="ParserOptions" /> instance.
        /// </summary>
        public ParserOptions() : this(new ErrorHandler())
        {
        }

        /// <summary>
        /// Create a new <see cref="ParserOptions" /> instance.
        /// </summary>
        /// <param name="source">A string representing where the code is coming from, if an error occurs.</param>
        public ParserOptions(string source) : this(new ErrorHandler { Source = source })
        {
        }

        /// <summary>
        /// Create a new <see cref="ParserOptions" /> instance.
        /// </summary>
        /// <param name="errorHandler">The <see cref="IErrorHandler" /> to use to handle errors.</param>
        public ParserOptions(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
        }

        /// <summary>
        /// Gets or sets whether the tokens are included in the parsed tree.
        /// </summary>
        public bool Tokens { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the comments are included in the parsed tree.
        /// </summary>
        public bool Comment { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the parser is tolerant to errors.
        /// </summary>
        public bool Tolerant { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="IErrorHandler"/> to use.
        /// </summary>
        public IErrorHandler ErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets whether the Regular Expression syntax should be converted to a .NET compatible one.
        /// </summary>
        public bool AdaptRegexp { get; set; } = true;

        /// <summary>
        /// Default timeout for created regexes, defaults to 10 seconds.
        /// </summary>
        public TimeSpan RegexTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The maximum depth of assignments allowed, defaults to 200.
        /// </summary>
        public int MaxAssignmentDepth { get; set; } = 200;

        /// Action to execute on each parsed node.
        /// </summary>
        /// <remarks>
        /// This callback allows you to make changes to the nodes created by the parser.
        /// E.g. you can use it to initialize <see cref="Node.Data"/> with a reference to the parent node:
        /// <code>
        /// options.OnNodeCreated = node => 
        /// { 
        ///    foreach (var child in node.ChildNodes)
        ///    {
        ///        if (child is not null)
        ///        {
        ///            child.Data = node;
        ///        }
        ///    }
        /// };
        /// </code>
        /// </remarks>
        public Action<Node>? OnNodeCreated { get; set; }

    }
}
