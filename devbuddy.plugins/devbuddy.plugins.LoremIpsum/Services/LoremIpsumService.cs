using System.Text;

namespace devbuddy.plugins.LoremIpsum.Services
{
    public class LoremIpsumService
    {
        private static readonly Random _random = new();

        private static readonly string[] _loremIpsumStart =
        {
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit."
        };

        private static readonly string[] _words =
        {
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "curabitur",
            "vel", "hendrerit", "libero", "eleifend", "blandit", "nunc", "ornare", "odio", "ut", "orci",
            "gravida", "imperdiet", "nullam", "purus", "lacinia", "a", "pretium", "quis", "congue",
            "praesent", "sagittis", "laoreet", "auctor", "mauris", "non", "velit", "eros", "dictum",
            "proin", "accumsan", "sapien", "nec", "massa", "volutpat", "venenatis", "sed", "eu",
            "molestie", "lacus", "quisque", "porttitor", "ligula", "dui", "mollis", "tempus", "at",
            "magna", "vestibulum", "turpis", "ac", "diam", "tincidunt", "id", "condimentum", "enim",
            "sodales", "in", "hac", "habitasse", "platea", "dictumst", "aenean", "neque", "fusce",
            "augue", "leo", "eget", "semper", "mattis", "tortor", "scelerisque", "nulla", "interdum",
            "tellus", "malesuada", "rhoncus", "porta", "sem", "aliquet", "et", "nam", "suspendisse",
            "potenti", "vivamus", "luctus", "fringilla", "erat", "donec", "justo", "vehicula", "ultricies",
            "varius", "ante", "primis", "faucibus", "ultrices", "posuere", "cubilia", "curae", "etiam",
            "cursus", "aliquam", "quam", "dapibus", "nisl", "feugiat", "egestas", "class", "aptent",
            "taciti", "sociosqu", "ad", "litora", "torquent", "per", "conubia", "nostra", "inceptos",
            "himenaeos", "phasellus", "nibh", "pulvinar", "vitae", "urna", "iaculis", "lobortis", "nisi",
            "viverra", "arcu", "morbi", "pellentesque", "metus", "commodo", "ut", "facilisis", "felis",
            "tristique", "ullamcorper", "placerat", "aenean", "convallis", "sollicitudin", "integer",
            "rutrum", "duis", "est", "etiam", "bibendum", "donec", "pharetra", "vulputate", "maecenas",
            "mi", "fermentum", "consequat", "suscipit", "aliquam", "habitant", "senectus", "netus",
            "fames", "quisque", "euismod", "curabitur", "lectus", "elementum", "tempor", "risus",
            "cras", "parturient", "montes", "nascetur", "ridiculus", "mus"
        };

        private static readonly string[] _latinWords =
        {
            "ad", "adipisci", "aliqua", "aliquam", "aliquet", "amet", "anim", "animi", "ante", "aperiam",
            "architecto", "asperiores", "aspernatur", "assumenda", "at", "atque", "autem", "beatae",
            "blanditiis", "cillum", "commodi", "consectetur", "consequatur", "consequuntur", "corporis",
            "corrupti", "culpa", "cupiditate", "debitis", "delectus", "deleniti", "deserunt", "dicta",
            "dignissimos", "distinctio", "dolor", "dolore", "dolorem", "doloremque", "dolores", "doloribus",
            "dolorum", "ducimus", "duis", "ea", "eaque", "earum", "eius", "eligendi", "enim", "eos",
            "error", "esse", "est", "et", "eu", "eum", "eveniet", "ex", "excepturi", "exercitationem",
            "expedita", "explicabo", "facere", "facilis", "fuga", "fugiat", "fugit", "harum", "hic",
            "id", "illo", "illum", "impedit", "in", "incidunt", "inventore", "ipsa", "ipsam", "ipsum",
            "irure", "iste", "itaque", "iure", "iusto", "labore", "laboriosam", "laboris", "laborum",
            "laudantium", "libero", "lorem", "magnam", "magni", "maiores", "maxime", "minim", "minima",
            "minus", "modi", "molestiae", "molestias", "mollit", "mollitia", "nam", "natus", "necessitatibus",
            "nemo", "neque", "nesciunt", "nihil", "nisi", "nobis", "non", "nostrum", "nulla", "occaecat",
            "odio", "odit", "officia", "officiis", "omnis", "optio", "pariatur", "perferendis", "perspiciatis",
            "placeat", "porro", "possimus", "praesentium", "proident", "provident", "quae", "quaerat",
            "quam", "quas", "quasi", "qui", "quia", "quibusdam", "quidem", "quis", "quisquam", "quo",
            "quod", "quos", "ratione", "recusandae", "reiciendis", "rem", "repellat", "repellendus",
            "reprehenderit", "repudiandae", "rerum", "saepe", "sapiente", "sed", "sequi", "similique",
            "sint", "sit", "soluta", "sunt", "suscipit", "tempora", "tempore", "temporibus", "tenetur",
            "totam", "ullam", "unde", "ut", "vel", "veniam", "veritatis", "vero", "vitae", "voluptas",
            "voluptate", "voluptatem", "voluptates", "voluptatibus", "voluptatum"
        };

        public string Generate(
            int paragraphs = 3,
            int sentencesPerParagraph = 5,
            int wordsPerSentence = 8,
            bool startWithLoremIpsum = true,
            bool includeLatinText = true)
        {
            if (paragraphs <= 0) paragraphs = 1;
            if (sentencesPerParagraph <= 0) sentencesPerParagraph = 1;
            if (wordsPerSentence <= 0) wordsPerSentence = 3;

            var result = new StringBuilder();
            var wordList = includeLatinText
                ? _words.Concat(_latinWords).ToArray()
                : _words;

            for (int p = 0; p < paragraphs; p++)
            {
                var paragraph = new StringBuilder();

                for (int s = 0; s < sentencesPerParagraph; s++)
                {
                    var sentence = new StringBuilder();

                    // Use the Lorem Ipsum start for the first sentence of the first paragraph if required
                    if (p == 0 && s == 0 && startWithLoremIpsum)
                    {
                        sentence.Append(_loremIpsumStart[0]);
                    }
                    else
                    {
                        // Generate a random sentence
                        int wordCount = _random.Next(wordsPerSentence - 2, wordsPerSentence + 2);
                        for (int w = 0; w < wordCount; w++)
                        {
                            if (w > 0) sentence.Append(" ");

                            string word = wordList[_random.Next(wordList.Length)];

                            // Capitalize first word of sentence
                            if (w == 0)
                            {
                                word = char.ToUpper(word[0]) + word.Substring(1);
                            }

                            sentence.Append(word);
                        }
                        sentence.Append(".");
                    }

                    if (s > 0) paragraph.Append(" ");
                    paragraph.Append(sentence);
                }

                result.AppendLine(paragraph.ToString());
                result.AppendLine();
            }

            return result.ToString().TrimEnd();
        }
    }
}
