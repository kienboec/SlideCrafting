# Markdown Cheat sheet 

### Basic constructs {.allowframebreaks}
- headings
- lists
- *italic*
- \*not italic\*
- **bold**
- [link](www.google.at)
- images
- tables
- footnote [^1]
- link with title: [Duck Duck Go](https://duckduckgo.com "The best search engine for privacy").
- auto link: http://www.example.com
- no link: `http://www.example.com`

[^1]: this is a footnote

Image
![Google Logo](https://www.google.at/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png){ width=100px }

Comments:

[info_comment_1]: <> (This is a comment, it will not be included)

[//]: # (This may be the most platform independent comment)

[](
Your comments go here however you cannot leave
// a blank line so fill blank lines with
//
Something 
)

term
: definition

~~The world is flat.~~

ruler 
---

ruler 2 

-----

- [x] Write the press release
- [ ] Update the website
- [ ] Contact the media



### a table with full border {.allowframebreaks} 
Less important, but don't ignore.

+----------+----------+
| type     | size [B] |
| :---     |     ---: |
+==========+==========+
| `byte`   | 1        |
+----------+----------+
| `short`  | 2        |
+----------+----------+
| `int`    | 4        |
+----------+----------+
| `long`   | 8        |
+----------+----------+
| `float`  | 4        |
+----------+----------+
| `double` | 8        |
+----------+----------+

### second table
second table with tabs (3rd layer header)

type  size
----- -----
byte  1
short 2  

### Code

~~~ {#SimpleFunctions .java .numberLines startFrom="1"}
public class JavaClass {
	
	public static void main(String[] args){
		//call function in other functions
		System.out.println("Hello World");
	}
}
~~~

### lists

> - x
> - y
> - z

::: nonincremental
- x
- y
- z
:::

::: incremental
- x
- y
- z
:::


### a Definition
Computer
: A computer is a programmable device that processes data using a finite set of instructions

### test pages

A content
this is content...

the ruler breaks the content to another slide

------------------ 

B content.

------------------

C content with notes

::: notes

This is my note.

- It can contain Markdown
- like this list

:::

-----------------



### blocks with pipe


| The Right Honorable Most Venerable and Righteous Samuel L. Constable, Jr.
| 200 Main St.
| Berkeley, CA 94718


### indented part {.allowframebreaks}
>~~~
> 24 -2  6  8 47 12
> 23 -3  5  7 46 11
>~~~

