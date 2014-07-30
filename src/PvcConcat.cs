using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcPlugins
{
    /// <summary>
    /// Concatenates multiple files onto a target file, then writes the results to a new file.
    /// </summary>
    public class PvcConcat : PvcPlugin
    {
        private readonly Func<PvcStream, bool> targetFilePredicate;
        private readonly string output;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilter">A filter function that selects which files will be concatenated.</param>
        /// <param name="targetFilePredicate">A predicate funtion that determines which file will be selected as the target file to which all others will be concatenated.</param>
        /// <param name="output">The output stream name.</param>
        public PvcConcat(
            Func<PvcStream, bool> targetFilePredicate,
            string output = "concat.js")
        {
            this.targetFilePredicate = targetFilePredicate;
            this.output = output;
        }

        public override string[] SupportedTags
        {
            get
            {
                return new[] { ".js" };
            }
        }

        public override IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams)
        {
            var targets = inputStreams.Where(s => this.targetFilePredicate(s)).ToList();
            if (targets.Count > 1)
            {
                throw new Exception("Multiple files match the target file condition.");
            }
            if (targets.Count == 0)
            {
                throw new Exception("Couldn't find any file that matches the target condition.");
            }

            var target = targets.Single();
            var sourceFiles = inputStreams
                .Except(targets)
                .ToList();
            
            var output = new MemoryStream();
            target.CopyTo(output);
            sourceFiles.ForEach(s => s.CopyTo(output));
            
            return inputStreams
                .Except(sourceFiles)
                .Concat(new[] { new PvcStream(() => output).As(this.output) });
        }
    }
}
