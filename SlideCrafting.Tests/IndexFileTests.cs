using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;

namespace SlideCrafting.Tests
{
    public class IndexFileTests
    {
        private readonly string _indexFile = @"
title: 'xyz'
subtitle: 'abc'
#date: '2020-07-10'

input-files:
- 04_programming/0.lst.yaml
- 04_programming/1.lst.yaml
- 04_programming/2.lst.yaml
- 04_programming/3.lst.yaml

lang: en-GB 

author: [ ""Kienboeck"" ]
institute: ""somewhere""

keywords: []

abstract: |


subject: asdf
description: A lecture

version: 0.1 
";

        [Test]
        public void Test1()
        {
            var input = new StringReader(_indexFile);

            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(input);

            var root = (YamlMappingNode)yaml.Documents[0].RootNode;
            YamlSequenceNode node = (YamlSequenceNode) root.Children["input-files"];
            Assert.AreEqual(4, node.Children.Count);
            CollectionAssert.AreEqual(new string[]
            {
                "04_programming/0.lst.yaml",
                "04_programming/1.lst.yaml",
                "04_programming/2.lst.yaml",
                "04_programming/3.lst.yaml",
            }, node.Children.Select(x => x.ToString()).ToArray());
        }
    }
}