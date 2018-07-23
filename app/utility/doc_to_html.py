import os
import sys

INDENT_CHAR = " "
INDENT_AMOUNT = 4


def convert_file_to_li(filename, output_filename):
    newfile = ["<ul>\n"]
    with open(filename, "r") as file:
        current_indentation = 0
        nested_indent = INDENT_AMOUNT

        for line in file.readlines():
            indentation = len(line) - len(line.lstrip())
            split = line.lstrip().split(' ', 1)
            newline = ""
            if indentation == current_indentation:
                newline = INDENT_CHAR * nested_indent + "<li><code>" + split[0] + "</code> " + split[1].rstrip() + "\n"
            elif indentation == current_indentation + INDENT_AMOUNT:
                current_indentation = indentation
                newfile.append(INDENT_CHAR * (indentation + INDENT_AMOUNT) + "<ul>\n")
                nested_indent = indentation + INDENT_AMOUNT * 2
                newline = INDENT_CHAR * nested_indent + "<li><code>" + split[0] + "</code> " + split[1].rstrip() + "\n"
            elif indentation == current_indentation - INDENT_AMOUNT:
                current_indentation = indentation
                newfile.append(INDENT_CHAR * (indentation + INDENT_AMOUNT * 2) + "</ul>\n")
                if not line.strip() == "PLACE HOLDER":
                    newfile.append(INDENT_CHAR * (indentation + INDENT_AMOUNT * 2) + "<li><code>" + split[0] + "</code> " + split[1].rstrip() + "\n")

            newfile.append(newline)
        newfile.append("</ul>")
    with open(output_filename, "w+") as outputfile:
        outputfile.writelines(newfile)


def convert_directory_to_li(dirname, output_dir):
    for filename in os.listdir(dirname):
        if (filename.endswith(".txt")):
            convert_file_to_li(filename, output_dir + filename.replace(".txt") + "_doc.html")


if __name__ == "__main__":
    usage = "\n\nUsage: python3 doc-to-html.py --directory|--file <path>\n\n"
    try:
        output_dir = "doc_texts/html/"
        mode = sys.argv[1]
        os.makedirs(output_dir, exist_ok=True)
        if mode == "--directory":
            convert_directory_to_li(sys.argv[2], output_dir)
        elif mode == "--file":
            convert_file_to_li(sys.argv[2], output_dir + sys.argv[2].split('/')[-1].replace(".txt", "") + "_doc.html")
        else:
            print(usage)
    except IndexError:
        print(usage)
