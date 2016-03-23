# FlipScript
A web tool that creates a series of swipable pages/panes from a [Markdown](http://commonmark.org/) file.

The idea is that it makes a long demo script easier to follow, but there are plenty of applications for any document that is written in Markdown that needs to be split based on headers.

Written in C# ASP.NET Core 1.0 RC1 MVC

You can see the site itself at [FlipScript.AzureWebsites.Net](http://FlipScript.AzureWebsites.net) which is deployed automatically based on the most recent commit to this repo

## Usage
You can use the website's main UI to upload Markdown files from your local device or GitHub. You can also pass a GitHub URL in the url as a Base64 encoded string using the following format:

```http://FlipScript.AzureWebsites.net/home/viewer/[Your Base64 encoded GitHub url]```

This is usefull if you want to save favourites/links that open a Markdown file with a single click. The github url must be Base64 encoded. There are lots of ways to encode strings ot Base64, but I use [Base64Encode.org](https://www.base64encode.org/).

For example, if you wanted a link to this Markdown file (the one you are reading right now), first gets its url which is `https://github.com/martinkearn/FlipScript/blob/master/README.md`

You then Base64 encode the url to get `aHR0cHM6Ly9naXRodWIuY29tL21hcnRpbmtlYXJuL0ZsaXBTY3JpcHQvYmxvYi9tYXN0ZXIvUkVBRE1FLm1k`

You can then construct your full FlipScript url which would be http://FlipScript.AzureWebsites.net/home/viewer/HR0cHM6Ly9naXRodWIuY29tL21hcnRpbmtlYXJuL0ZsaXBTY3JpcHQvYmxvYi9tYXN0ZXIvUkVBRE1FLm1k
