var PORT = 8080;

var fs = require('fs')
var path = require('path')
var express = require('express');
var app = express();
var morgan = require('morgan')

// create a write stream (in append mode)
var accessLogStream = fs.createWriteStream('/miktex/work/log/access.log', { flags: 'a' });

app.use(morgan(':date[iso]: :status :method :url size: :res[content-length] \ttime: :response-time ms', { stream: accessLogStream }));
app.use(express.static('/miktex/work/dist/'));
app.use(express.static('/miktex/work/log/'));
// app.use(express.static('/miktex/.miktex/texmfs/data/miktex/log/pdflatex.log'));

app.listen(PORT, function () {
  console.log('SlideCrafting HTTP-server app listening on port ' + PORT + '!');
});
