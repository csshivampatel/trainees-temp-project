using System;
using System.Activities;
using System.Activities.Validation;
using System.Globalization;
using System.Text;

namespace SampleDataActivityLibrary
{
    /// <summary>
    ///     Activity that generates lorem ipsum text.
    /// </summary>
    public sealed class LoremIpsum : CodeActivity<string>
    {
        public LoremIpsum()
        {
            MinWords = 5;
            MaxWords = 10;
            MinSentences = 3;
            MaxSentences = 6;
            Paragraphs = 1;
        }

        // Design-time properties
        public int MaxSentences { get; set; }
        public int MaxWords { get; set; }
        public int MinSentences { get; set; }
        public int MinWords { get; set; }
        public int Paragraphs { get; set; }


        /// <summary>
        ///     Creates and validates a description of the activity’s arguments, variables, child activities, and activity
        ///     delegates.
        /// </summary>
        /// <param name="metadata">
        ///     The activity’s metadata that encapsulates the activity’s arguments, variables, child activities,
        ///     and activity delegates.
        /// </param>
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            // validate properties
            if (MinWords < 1)
            {
                metadata.AddValidationError(new ValidationError("MinWords must be at least 1."));
            }

            if (MaxWords < 1)
            {
                metadata.AddValidationError(new ValidationError("MaxWords must be at least 1."));
            }

            if (MinSentences < 1)
            {
                metadata.AddValidationError(new ValidationError("MinSentences must be at least 1."));
            }

            if (MaxSentences < 1)
            {
                metadata.AddValidationError(new ValidationError("MaxSentences must be at least 1."));
            }

            if (Paragraphs < 1)
            {
                metadata.AddValidationError(new ValidationError("Paragraphs must be at least 1."));
            }

            if (MaxWords < MinWords)
            {
                metadata.AddValidationError(new ValidationError("MaxWords must be greater than or equal to MinWords."));
            }

            if (MaxSentences < MinSentences)
            {
                metadata.AddValidationError(
                    new ValidationError("MaxSentences must be greater than or equal to MinSentences."));
            }
        }

        /// <summary>
        ///     The code that is executed when the workflow activity is run.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        /// <returns>The lorem ipsum text.</returns>
        protected override string Execute(CodeActivityContext context)
        {
            var text = GeneateLoremIpsum(MinWords, MaxWords, MinSentences, MaxSentences, Paragraphs);
            return text;
        }


        /// <summary>
        ///     Generates Lorem Ipsum text base on the generation criteria specified.
        /// </summary>
        /// <param name="minWords">The minimum number of words.</param>
        /// <param name="maxWords">The maximum number of words.</param>
        /// <param name="minSentences">The minimum number of sentences.</param>
        /// <param name="maxSentences">The maximum number of sentences.</param>
        /// <param name="numParagraphs">The number of paragraphs to generate.</param>
        /// <returns>The lorem ipsum text.</returns>
        private static string GeneateLoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences,
            int numParagraphs)
        {
            var words = new[]
            {
                "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
                "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
                "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
            };

            var rand = new Random();

            var sb = new StringBuilder();

            for (int p = 0; p < numParagraphs; p++)
            {
                int numSentences = rand.Next(minSentences, maxSentences + 1);

                if (p > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }

                for (int s = 0; s < numSentences; s++)
                {
                    int numWords = rand.Next(minWords, maxWords + 1);

                    for (int w = 0; w < numWords; w++)
                    {
                        if (w > 0)
                        {
                            sb.Append(" ");
                        }

                        if (w == 0)
                        {
                            var word = words[rand.Next(words.Length)];

                            sb.Append(word[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant() +
                                      word.Substring(1));
                        }
                        else
                        {
                            sb.Append(words[rand.Next(words.Length)]);
                        }
                    }
                    sb.Append(". ");
                }
            }

            return sb.ToString();
        }
    }
}