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
        /// <returns></returns>
        public ParserOptions(string source) : this(new ErrorHandler {Source = source})
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
        /// Gets or sets whether each node should have their range included.
        /// </summary>
        /// <value></value>
        public bool Range { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the parsed elements have their location included.
        /// </summary>
        public bool Loc { get; set; } = false;

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
        public bool AdaptRegexp { get; set; }
    }
}
