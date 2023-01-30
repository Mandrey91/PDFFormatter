using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace PDF
{
    public class PDFFormatter
    {
        private const char NonBreakingSpace = '\u00A0';


        /// <summary>текущий номер раздела в тексте</summary>
        private static int _sectionNumber;
        /// <summary>текущий номер рисунка в тексте<</summary>
        private static int _pictureNumber;
        /// <summary>текущий номер таблиц в тексте<</summary>
        private static int _tableNumber;
        /// <summary>нумерация источников в списке литературы</summary>
        private int _sourceNumber;
        /// <summary>путь до исходного шаблона</summary>
        private string sourcePath;
        /// <summary>путь до выходного файла</summary>
        private string distPath;
        /// <summary>список шаблонных строк в тексте для форматирования</summary>
        private List<string> templates = new List<string>();
        /// <summary>список литературы</summary>
        private List<string> sourceList = new List<string>();

        private readonly Dictionary<string, Color> _colors;

        public PDFFormatter()
        {
            _sectionNumber = 0;
            _pictureNumber = 0;
            _tableNumber = 0;
            _sourceNumber = 0;
            sourcePath = @"input.txt";
            distPath = @"result.pdf";
            templates = new List<string>()
            {
                "[*номер раздела*]", //0
                "[*номер рисунка*]", //1
                "[*номер таблицы*]", //2
                "[*ссылка на следующий рисунок*]", //3
                "[*ссылка на предыдущий рисунок*]", //4
                "[*ссылка на таблицу*]", //5
                "[*таблица ", //6
                "[*cписок литературы*]", //7
                "[*код", //8
                "[*рисунок " //9
            };
            sourceList = new List<string>();

            _colors = new Dictionary<string, Color>()
            {
                { "red", Color.RED },
                { "orange", Color.ORANGE },
                { "yellow", Color.YELLOW },
                { "green", Color.GREEN },
                { "blue", Color.BLUE },
                { "pink", Color.PINK },
                { "black", Color.BLACK },
                { "white", Color.WHITE },
            };
        }

        public void Make()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            //CODEPART 1 Открытие документа и задание его формата, подготовка
            System.IO.FileStream fs = new System.IO.FileStream(distPath, System.IO.FileMode.Create);
            //обозначаем размер полей
            float leftMargin = 50f;
            float rightMargin = 50f;
            //создаем документ
            Document document = new Document(PageSize.A4, leftMargin, rightMargin, leftMargin,
            rightMargin);
            //связываем документ с файлом
            PdfWriter writer = PdfWriter.GetInstance(document, fs);
            //открываем документ
            document.Open();
            //определяем шрифт
            float fontSizeText = 12f;
            BaseFont baseFont = BaseFont.CreateFont(
                new FileInfo(sourcePath).DirectoryName + "\\" + @"ARIAL.TTF",
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);
            //считываем все строки из текстового файла
/*            IEnumerable<string> paragraphs = 
                .Select(line => RemoveDoubleWhitespaces(line));*/
            IEnumerable<string> paragraphs = NormalizeText(File.ReadAllLines(sourcePath));

            //CODEPART 2 обходим все строки файла - параграфы
            foreach (string paragraph in paragraphs)
            {
                //вставлен ли уже параграф в PDF
                bool isSetParagraph = false;
                //текущий текст параграфа
                string textParagraph = paragraph;
                //проверяем, входит ли в параграф ключевое слово
                foreach (var template in templates)
                {
                    if (paragraph.Contains(template) == false)
                        continue;

                    var i = templates.ToList().IndexOf(template);
                    switch (i)
                    {
                        //CODEPART 2.1 Редактирование абзаца заголовка раздела
                        case 0:// "[*номер раздела*]"
                            textParagraph = ProcessPageNumberCase(document, baseFont, textParagraph, template);

                            //абзац уже вставлен
                            isSetParagraph = true;
                            //TODO (задание на 5) дополните код и шаблон, чтобы велась нумерация подразделов, пунктов,
                            //подпунктов со своим форматированием
                            //1 раздел
                            //1.1 подраздел
                            //1.1.1 пункт
                            //1.1.1.1 подпункт

                            break;
                        //CODEPART 2.1 Редактирование подрисуночной подписи

                        case 1://"[*номер рисунка*]"
                            textParagraph = ProcessPictureNumberCase(document, fontSizeText, baseFont, textParagraph, template);
                            isSetParagraph = true;

                            break;

                        //CODEPART 2.3 Редактирование заголовка таблицы

                        case 2://"[*номер таблицы*]"
                            textParagraph = ProcessTableNumberCase(document, fontSizeText, baseFont, textParagraph, template);
                            //абзац уже вставлен
                            isSetParagraph = true;

                            break;

                        //CODEPART 2.4 Вставка ссылки на следующий рисунок
                        case 3://"[*ссылка на следующий рисунок*]"
                            textParagraph = ProcessNextPictureLinkCase(textParagraph, template);

                            break;
                        //CODEPART 2.5 Вставка ссылки на предыдущий рисунок

                        case 4://"[*ссылка на таблицу*]
                            textParagraph = ProcessCurrentPictureLinkCase(textParagraph, template);

                            break;

                        //CODEPART 2.6 Вставка ссылки на таблицу
                        case 5://"[*ссылка на таблицу*]"
                            textParagraph = ProcessTableLinkCase(textParagraph, template);

                            break;

                        //CODEPART 2.7 Вставка таблицы из файла

                        case 6://"[*таблица "
                            ProcessTableInsertionCase(document, fontSizeText, baseFont, textParagraph, template);
                            //TODO (задание на 4) применить свое форматирование к таблице: границы, шрифт, цвет шрифта и заливки
                            //абзац уже вставлен
                            isSetParagraph = true;

                            break;

                        //CODEPART 2.8 Вставка списка литературы
                        case 7://"[*cписок литературы*]"
                            ProcessSourcesInsertionCase(document, fontSizeText, baseFont);

                            //абзац уже вставлен
                            isSetParagraph = true;
                            //TODO (задание на 5) если полнотекстовая ссылка содержит url (начивается с http), то вставить
                            //дополнение
                            //Название страницы [Электронный источник] // Название сайта, текущий год. Режим доступа: URL (дата
                            //обращения: текущая дата).

                            break;

                        //CODEPART 2.9 Вставка кода из файла
                        case 8://"[*код"
                            //если есть шаблонная строка для места вставки кода
                            textParagraph = "тут будет ваш код";
                            //TODO (задание на 5) вставить код из файла - CourierNew 8 пт одинарный без отступа в рамке
                            break;

                        //CODEPART 2.10 Вставка рисунка из файл

                        case 9://"[*таблица "
                            ProcessPictureInsertionCase(document, textParagraph, template);
                            //абзац уже вставлен
                            isSetParagraph = true;
                            
                            break;

                    }

                }
                //CODEPART 2.11 Сбор внутритекстовых ссылок на литературу
                textParagraph = UpdateLineAndSourcesList(textParagraph);
                if (!isSetParagraph)
                {
                    //вставляем абзац со стандарным форматированием
                    var iparagraph = new Paragraph(textParagraph,

                    new Font(baseFont, fontSizeText, Font.NORMAL));
                    iparagraph.SpacingAfter = 0;
                    iparagraph.SpacingBefore = 0;
                    iparagraph.FirstLineIndent = 20f;
                    iparagraph.ExtraParagraphSpace = 10;
                    iparagraph.Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(iparagraph);
                }
            }
            //закрываем документ
            document.Close();
        }

        private static IEnumerable<string> NormalizeText(IEnumerable<string> lines)
        {
            return lines
                .Select(line => RemoveDoubleWhitespaces(line))
                .Select(line => line.Replace("[", $"{NonBreakingSpace}["));
        }

        private static string RemoveDoubleWhitespaces(string text)
        {
           return Regex.Replace(text, @"\s+", " ");
        }

        private string UpdateLineAndSourcesList(string textParagraph)
        {
            //начинаем проверять внутритекстовые ссылки на литературу
            string text = textParagraph;
            //если есть открывающая скобка
            if (text.Contains("["))
            {
                //посимвольно проходим весь абзац
                for (int j = 0; j < text.Length - 1; j++)
                {
                    //если нашли открывающую скобку без последующего символа *
                    if (text[j] == '[' && text[j + 1] != '*')
                    {

                        //то начинаем искать закрывающую скобку
                        int startIndex = j;
                        int endIndex = startIndex + 1;
                        while (endIndex < text.Length && text[endIndex] != ']')
                        {
                            endIndex++;
                        }
                        string sourceName = "";
                        //если нашли закрывающую скобку (а не до конца абзаца)

                        if (text[endIndex] == ']')
                        {
                            //собираем текст между скобками (включая)
                            for (int k = startIndex; k <= endIndex; k++)
                            {
                                sourceName += text[k];
                            }

                            int index = 0;

                            //если не удалость перевести строку в цифру
                            //то значит это полный текст ссылки
                            //тогда, если в списке литературы нет еще такой ссылки
                            if (!sourceList.Contains(sourceName))
                            {
                                //добавляем в список, увеличиваем номер текущей ссылки

                                sourceList.Add(sourceName);
                                _sourceNumber++;
                                index = _sourceNumber;
                            }
                            else
                            {
                                //если же уже источник есть в списке
                                for (int k = 0; k < sourceList.Count; k++)
                                {
                                    //то находим его номер
                                    if (sourceList[k].Contains(sourceName))
                                    {
                                        index = k + 1;
                                    }
                                }
                            }
                            //ограничиваем номер ссылки в квадратные скобки
                            string replaceString = "[" + index.ToString() + "]";

                            //заменяем полнотекстовую ссылку на номер
                            textParagraph = textParagraph.Replace(sourceName, replaceString);
                            //двигаемся дальше по абцазу

                            j = endIndex;
                        }
                    }
                }

            }

            return textParagraph;
        }

        private void ProcessPictureInsertionCase(Document document, string textParagraph, string template)
        {
            //по формату мы задаем, что у нас есть шаблоная строка
            //[*рисунок XXXXX*] где XXXXX - имя файла с рисунком

            //поэтому эту строку мы должны извлечь

            //при этому убираем ненужные части шаблонной строки
            string jpgPath = textParagraph
                .Replace(template, "")
                .Replace("*", "")
                .Replace("\r", "")
                .Replace("]", "")
                .Replace(NonBreakingSpace.ToString(), "");


            //файл должен лежать рядом с исходным документом
            //поэтому определим полный путь (извлекаем путь до директории текущего документа)
            jpgPath = new System.IO.FileInfo(sourcePath).DirectoryName
            + "\\" + jpgPath;

            //создаем рисунок
            Image jpg = Image.GetInstance(jpgPath);
            jpg.Alignment = Element.ALIGN_CENTER;

            jpg.SpacingBefore = 12f;
            //уменьшаем размер рисунка до 50% ширины страницы
            float procent = 90;
            while (jpg.ScaledWidth > PageSize.A4.Width / 2.0f)

            {
                jpg.ScalePercent(procent);

                procent -= 10;

            }

            //добавляем рисунок
            document.Add(jpg);
        }

        private void ProcessSourcesInsertionCase(Document document, float fontSizeText, BaseFont baseFont)
        {
            //если есть шаблонная строка для места вставки списка литературы
            //собираем список литературы в многострочную строку

            string replaceString = "";

            for (int j = 0; j < sourceList.Count; j++)
            {
                replaceString = (j + 1).ToString() + ". "
                + sourceList[j].TrimStart('[').TrimEnd(']') + "\r\n";
                //вставляем абзац
                var iparagraph = new Paragraph(replaceString,
                new Font(baseFont, fontSizeText, Font.NORMAL));
                iparagraph.SpacingAfter = 0;
                iparagraph.SpacingBefore = 0;
                iparagraph.FirstLineIndent = 20f;
                iparagraph.ExtraParagraphSpace = 10;
                iparagraph.Alignment = Element.ALIGN_JUSTIFIED;

                document.Add(iparagraph);
            }
        }

        private void ProcessTableInsertionCase(Document document, float fontSizeText, BaseFont baseFont, string textParagraph, string template)
        {
            template = template.Replace("[*", "");
            char[] separators = ";,".ToCharArray();

            //по формату мы задаем, что у нас есть шаблоная строка
            //[*таблица XXXXX*] где XXXXX - имя файла csv с таблицей
            //поэтому эту строку мы должны извлечь
            //при этому убираем ненужные части шаблонной строки

            string csvPath = GetStringBetween(textParagraph, "*", "*")
                .Replace(template, "");

            bool areParametersSpecified = false;
            string parametersString = GetStringBetween(textParagraph, "(", ")");
            string[] parameters = parametersString
                .Replace(" ", "")
                .Split(separators);

            if (String.IsNullOrEmpty(parametersString) == false)
            {
                if (parameters.Length != 4 || parameters.Any(parameter => String.IsNullOrEmpty(parameter)))
                    throw new InvalidOperationException("Неверно указанны параметры таблицы");
                
                areParametersSpecified = true;
            }

            int border = areParametersSpecified ? int.Parse(parameters[0]) : 15;
            float fontSize = areParametersSpecified ? float.Parse(parameters[1]) : fontSizeText;
            Color fontColor = areParametersSpecified ? _colors[parameters[2].ToLower()] : Color.BLACK;
            Color backgroundColor = areParametersSpecified ? _colors[parameters[3].ToLower()] : Color.WHITE;


            //файл должен лежать рядом с исходным документом
            //поэтому определим полный путь (извлекаем путь до директории текущего документа)
            csvPath = new FileInfo(sourcePath).DirectoryName + "\\" + csvPath;
            //считываем строки таблицы
            string[] lines = File.ReadAllLines(csvPath);
            //делим первую строку на ячейки - заголовки таблицы
            string[] titles = lines[0].Split(
                separators,
                StringSplitOptions.RemoveEmptyEntries
                );

            //создаем таблицу с указанием количества колонок
            PdfPTable table = new PdfPTable(titles.Length);

            var font = new Font(baseFont, fontSize, Font.NORMAL, fontColor);

            //заполняем заголовки таблицы
            foreach (string title in titles)
            {
                var phrase = new Phrase(title, font);
                var cell = new PdfPCell(phrase);

                cell.Border = border;
                cell.BackgroundColor = backgroundColor;

                table.AddCell(cell);
            }

            //заполняем таблицу
            for (int row = 1; row < lines.Length; row++)
            {
                string[] rowValues = lines[row].Split(
                    separators,
                    StringSplitOptions.RemoveEmptyEntries
                    );

                foreach (string value in rowValues)
                {
                    var phrase = new Phrase(value, font);
                    var cell = new PdfPCell(phrase);

                    cell.Border = border;
                    cell.BackgroundColor = backgroundColor;

                    table.AddCell(cell);
                }
            }

            //добавляем таблицу в документ
            document.Add(table);
        }

        private static string ProcessTableLinkCase(string textParagraph, string template)
        {
            //заменяем текст на номер следующей таблицы
            string replaceString = _sectionNumber.ToString() + "." + (_tableNumber + 1).ToString();
            textParagraph = textParagraph.Replace(template, replaceString);
            return textParagraph;
        }

        private static string ProcessCurrentPictureLinkCase(string textParagraph, string template)
        {
            //заменяем текст на текущий номер рисунка
            string replaceString = _sectionNumber.ToString() + "." + _pictureNumber.ToString();
            textParagraph = textParagraph.Replace(template, replaceString);
            return textParagraph;
        }

        private static string ProcessNextPictureLinkCase(string textParagraph, string template)
        {
            //заменяем текст на следующий номер рисунка
            string replaceString = _sectionNumber.ToString() + "." + (_pictureNumber + 1).ToString();
            textParagraph = textParagraph.Replace(template, replaceString);
            return textParagraph;
        }

        private static string ProcessTableNumberCase(Document document, float fontSizeText, BaseFont baseFont, string textParagraph, string template)
        {
            _tableNumber++;//номер таблицы состоит из номера раздела и номера таблицы
            string replaceString = "Таблица " + _sectionNumber.ToString()
            + "." + _tableNumber.ToString() + " –";
            textParagraph = textParagraph.Replace(template, replaceString);
            var iparagraph = new Paragraph(textParagraph,
            new Font(baseFont, fontSizeText, Font.ITALIC));

            iparagraph.SpacingAfter = 12f;
            iparagraph.Alignment = Element.ALIGN_LEFT;
            document.Add(iparagraph);
            return textParagraph;
        }

        private static string ProcessPictureNumberCase(Document document, float fontSizeText, BaseFont baseFont, string textParagraph, string template)
        {
            //увеличиваем номер рисунка
            _pictureNumber++;
            //составляем номер рисунка из номера раздела и номера рисунка в разделе
            string replaceString = "Рисунок " + _sectionNumber.ToString()
            + "." + _pictureNumber.ToString() + " –";
            //заменяем вхождение ключевого слова на номер
            textParagraph = textParagraph.Replace(template, replaceString);
            //вставляем абзац текста
            var iparagraph = new Paragraph(textParagraph,
            new Font(baseFont, fontSizeText, Font.ITALIC));
            iparagraph.SpacingAfter = 12f;
            iparagraph.Alignment = Element.ALIGN_CENTER;
            document.Add(iparagraph);
            return textParagraph;
        }

        private static string ProcessPageNumberCase(Document document, BaseFont baseFont, string textParagraph, string template)
        {
            //увеличиваем номер раздела, начинаем нумерацию рисунков и таблиц заново
            //так как из нумерация сквозная по разделу
            _sectionNumber++;


            _pictureNumber = 0;
            _tableNumber = 0;
            //определяем строку для замены ключевого слова на номер
            string replaceString = _sectionNumber.ToString();
            //заменяем вхождение ключевого слова на номер
            textParagraph = textParagraph.Replace(template, replaceString);
            //если не первый раздел, делаем разрыв
            if (_sectionNumber != 1)
            {
                document.NewPage();
            }
            //вставляем абзац текста
            var iparagraph = new Paragraph(
                textParagraph,
                new Font(baseFont, 13f, Font.BOLD)
                );
            iparagraph.SpacingAfter = 15f;
            iparagraph.ExtraParagraphSpace = 10;
            iparagraph.Alignment = Element.ALIGN_CENTER;
            document.Add(iparagraph);

            Chapter chapter = new Chapter(iparagraph, _sectionNumber);
            document.Add(chapter);
            return textParagraph;
        }

        private static string GetStringBetween(string text, string left, string right)
        {
            int leftIndex = text.IndexOf(left) + left.Length;
            int rightIndex = leftIndex + text.Substring(leftIndex).IndexOf(right);

            int length = rightIndex - leftIndex;

            if (length > 0)
                return text.Substring(leftIndex, length);
            else
                return String.Empty;
        }
    }
}