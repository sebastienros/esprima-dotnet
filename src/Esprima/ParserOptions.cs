namespace Esprima
{
    public class ParserOptions
    {
        public ParserOptions()
        {
            ErrorHandler = new ErrorHandler();
        }

        public ParserOptions(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
        }

        public bool Range { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the parsed elements have their location included.
        /// </summary>
        public bool Loc { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the source code.
        /// </summary>
        public string Source { get; set; }

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
        /// Gets or sets whether the source represents a module or a script.
        /// </summary>
        public SourceType SourceType { get; set; } = SourceType.Script;

        /// <summary>
        /// Gets or sets the <see cref="IErrorHandler"/> to use.
        /// </summary>
        public IErrorHandler ErrorHandler { get; set; }
    }
}
