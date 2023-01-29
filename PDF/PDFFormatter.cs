using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace PDF
{
    public class PDFFormatter
    {
        /// <summary>текущий номер раздела в тексте</summary>
        static int _sectionNumber = 0;
        /// <summary>текущий номер рисунка в тексте<</summary>
        static int _pictureNumber = 0;
        /// <summary>текущий номер таблиц в тексте<</summary>
        static int _tableNumber = 0;
        /// <summary>нумерация источников в списке литературы</summary>
        int _sourceNumber = 0;
        /// <summary>путь до исходного шаблона</summary>
        string sourcePath = @"input.txt";
        /// <summary>путь до выходного файла</summary>
        string distPath = @"result.pdf";
        /// <summary>список шаблонных строк в тексте для форматирования</summary>
        string[] templateStringList =
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
        /// <summary>список литературы</summary>
        List<string> sourceList = new List<string>();
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
            new System.IO.FileInfo(sourcePath).DirectoryName + "\\" + @"ARIAL.TTF",
            BaseFont.IDENTITY_H,
            BaseFont.NOT_EMBEDDED);
            //считываем все строки из текстового файла
            string[] paragraphs = System.IO.File.ReadAllLines(sourcePath);
            //CODEPART 2 обходим все строки файла - параграфы
            foreach (string paragraph in paragraphs)
            {
                //вставлен ли уже параграф в PDF
                bool isSetParagraph = false;
                //текущий текст параграфа
                string textParagraph = paragraph;
                //проверяем, входит ли в параграф ключевое слово
                for (int i = 0; i < templateStringList.Length; i++)

                {

                    if (paragraph.Contains(templateStringList[i]))

                    {

                        switch (i)
                        {

                            //CODEPART 2.1 Редактирование абзаца заголовка раздела

                            case 0:// "[*номер раздела*]"
                                {
                                    //увеличиваем номер раздела, начинаем нумерацию рисунков и таблиц заново
                                    //так как из нумерация сквозная по разделу
                                    _sectionNumber++;


                                    _pictureNumber = 0;
                                    _tableNumber = 0;
                                    //определяем строку для замены ключевого слова на номер
                                    string replaceString = _sectionNumber.ToString();
                                    //заменяем вхождение ключевого слова на номер
                                    textParagraph = textParagraph.Replace(templateStringList[i], replaceString);
                                    //если не первый раздел, делаем разрыв
                                    if (_sectionNumber != 1)
                                    {
                                        document.NewPage();
                                    }
                                    //вставляем абзац текста
                                    var iparagraph = new Paragraph(textParagraph,
                                    new Font(baseFont, 13f, Font.BOLD));
                                    iparagraph.SpacingAfter = 15f;
                                    iparagraph.ExtraParagraphSpace = 10;
                                    iparagraph.Alignment = Element.ALIGN_CENTER;
                                    document.Add(iparagraph);

                                    Chapter chapter = new Chapter(iparagraph, _sectionNumber);
                                    document.Add(chapter);

                                    //абзац уже вставлен
                                    isSetParagraph = true;
                                    //TODO (задание на 5) дополните код и шаблон, чтобы велась нумерация подразделов, пунктов,
                                    //подпунктов со своим форматированием
                                    //1 раздел
                                    //1.1 подраздел
                                    //1.1.1 пункт
                                    //1.1.1.1 подпункт
                                }
                                break;
                            //CODEPART 2.1 Редактирование подрисуночной подписи

                            case 1://"[*номер рисунка*]"
                                {
                                    //увеличиваем номер рисунка
                                    _pictureNumber++;
                                    //составляем номер рисунка из номера раздела и номера рисунка в разделе
                                    string replaceString = "Рисунок " + _sectionNumber.ToString()
                                    + "." + _pictureNumber.ToString() + " –";
                                    //заменяем вхождение ключевого слова на номер
                                    textParagraph = textParagraph.Replace(templateStringList[i], replaceString);
                                    //вставляем абзац текста
                                    var iparagraph = new Paragraph(textParagraph,
                                    new Font(baseFont, fontSizeText, Font.ITALIC));
                                    iparagraph.SpacingAfter = 12f;
                                    iparagraph.Alignment = Element.ALIGN_CENTER;
                                    document.Add(iparagraph);
                                    isSetParagraph = true;
                                }
                                break;

                            //CODEPART 2.3 Редактирование заголовка таблицы

                            case 2://"[*номер таблицы*]"
                                {
                                    _tableNumber++;//номер таблицы состоит из номера раздела и номера таблицы
                                    string replaceString = "Таблица " + _sectionNumber.ToString()
                                    + "." + _tableNumber.ToString() + " –";
                                    textParagraph = textParagraph.Replace(templateStringList[i], replaceString);
                                    var iparagraph = new Paragraph(textParagraph,
                                    new Font(baseFont, fontSizeText, Font.ITALIC));

                                    iparagraph.SpacingAfter = 12f;
                                    iparagraph.Alignment = Element.ALIGN_LEFT;
                                    document.Add(iparagraph);
                                    //абзац уже вставлен
                                    isSetParagraph = true;
                                }
                                break;
                            //CODEPART 2.4 Вставка ссылки на следующий рисунок
                            case 3://"[*ссылка на следующий рисунок*]"
                                {
                                    //заменяем текст на следующий номер рисунка
                                    string replaceString = _sectionNumber.ToString() + "." + (_pictureNumber + 1).ToString();
                                    textParagraph = textParagraph.Replace(templateStringList[i], replaceString);
                                }
                                break;
                            //CODEPART 2.5 Вставка ссылки на предыдущий рисунок

                            case 4://"[*ссылка на таблицу*]
                                {
                                    //заменяем текст на текущий номер рисунка
                                    string replaceString = _sectionNumber.ToString() + "." + _pictureNumber.ToString();
                                    textParagraph = textParagraph.Replace(templateStringList[i], replaceString);
                                }
                                break;

                            //CODEPART 2.6 Вставка ссылки на таблицу
                            case 5://"[*ссылка на таблицу*]"
                                {
                                    //заменяем текст на номер следующей таблицы
                                    string replaceString = _sectionNumber.ToString() + "." + (_tableNumber + 1).ToString();
                                    textParagraph = textParagraph.Replace(templateStringList[i], replaceString);
                                }
                                break;

                            //CODEPART 2.7 Вставка таблицы из файла

                            case 6://"[*таблица "

                                {
                                }
                                break;
                            //CODEPART 2.8 Вставка списка литературы
                            case 7://"[*cписок литературы*]"

                                {
                                }
                                break;

                            //CODEPART 2.9 Вставка кода из файла

                            case 8://"[*код"
                                {
                                    //если есть шаблонная строка для места вставки кода
                                    textParagraph = "тут будет ваш код";
                                    //TODO (задание на 5) вставить код из файла - CourierNew 8 пт одинарный без отступа в рамке
                                }
                                break;

                            //CODEPART 2.10 Вставка рисунка из файл

                            case 9://"[*таблица "
                                {
                                    //по формату мы задаем, что у нас есть шаблоная строка
                                    //[*рисунок XXXXX*] где XXXXX - имя файла с рисунком

                                    //поэтому эту строку мы должны извлечь

                                    //при этому убираем ненужные части шаблонной строки
                                    string jpgPath = textParagraph.Replace(templateStringList[i],
                                    "").Replace("*", "").Replace("\r", "").Replace("]", "");


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
                                    //абзац уже вставлен
                                    isSetParagraph = true;
                                }
                                break;

                        }
                    }
                }
                //CODEPART 2.11 Сбор внутритекстовых ссылок на литературу
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
    }
}