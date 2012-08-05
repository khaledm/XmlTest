using System;
using System.IO;
using System.Reflection;
using System.Xml;
using NConsoler;

namespace TestConsole
{
    internal class Program
    {
        #region Global Variables
        private static readonly string XmlFilePath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\", @"BookStore.xml");

        private static XmlNamespaceManager _gNsMngr;
        private static readonly XmlDocument _gBookStoreXdoc = new XmlDocument();
        #endregion

        private static void InitXmlDoc()
        {
            _gBookStoreXdoc.Load(XmlFilePath);
            _gNsMngr = new XmlNamespaceManager(_gBookStoreXdoc.NameTable);
            _gNsMngr.AddNamespace("nsBooks", "http://www.contoso.com/books");
            _gNsMngr.AddNamespace("nsAuthors", "http://www.contoso.com/author");
            _gNsMngr.AddNamespace("nsGenre", "http://www.contoso.com/genre");
            _gNsMngr.PushScope();
        }


        private static void Main(string[] args)
        {
            InitXmlDoc();
            Consolery.Run(typeof(Program), args);
        }

        [Action]
        public static void ShowBooksUnderGbp(
            [Required] string price) //Nconsoler doesn't allow decimal as action param; see manual for others.
        {
            //Show all books under amount X

            Console.WriteLine("---------------------\nShow books < gbp {0}\n---------------", decimal.Parse(price));

            string xPriceFilterExpr = string.Format("descendant::nsBooks:book[nsBooks:price < {0}]",
                                                    decimal.Parse(price));

            XmlNodeList books = _gBookStoreXdoc.DocumentElement.SelectNodes(xPriceFilterExpr, _gNsMngr);

            if (books == null || books.Count == 0)
            {
                Console.WriteLine("Books < gbp {0} could not be found...", price);
            }

            foreach (XmlNode book in books)
            {
                PrintBook(book);
            }
        }


        [Action]
        public static void ShowBooksByGenre(
            [Required] string genre)
        {
            //show books by genre
            Console.WriteLine("---------------------\nShow books by genre {0}\n---------------", genre);

            string xGenreFilterExpr = string.Format("descendant::nsBooks:book[@nsGenre:genre = '{0}']", genre);
            XmlNodeList books = _gBookStoreXdoc.DocumentElement.SelectNodes(xGenreFilterExpr, _gNsMngr);

            if (books == null || books.Count == 0)
            {
                //throw exception?
                Console.WriteLine("Books of genre '{0}' not be found...", genre);
            }

            foreach (XmlNode book in books)
            {
                PrintBook(book);
            }
        }


        [Action]
        public static void ShowBooksByAuthorsFirstNameLike(
            [Required] string nameChar)
        {
            //show all books where authors first name has an 'a' in it

            Console.WriteLine(
                "---------------------\nShow books by authors with first name has '{0}'\n---------------", nameChar);

            string xNameFilterExpr =
                "descendant::nsBooks:book/descendant::nsAuthors:author/descendant::nsAuthors:first-name[contains(.,'" +nameChar + "')]/parent::node()/parent::node()";

            XmlNodeList books = _gBookStoreXdoc.DocumentElement.SelectNodes(xNameFilterExpr, _gNsMngr);
            if (books == null || books.Count == 0)
            {
                //throw exception?
                Console.WriteLine("Books of authors where FirstName has '{0}' could not be found...", nameChar);
            }
            foreach (XmlNode book in books)
            {
                PrintBook(book);
            }
        }


        [Action]
        public static void FindAveragePriceByGenreAndPriceGreaterThanGbp(
            [Required] string genre,
            [Required] string price
            )
        {
            Console.WriteLine(
                "---------------------\nFind average price by genre {0} and price > gbp {1} \n---------------", genre,
                decimal.Parse(price));

            string avgPriceCalcExpr =
                String.Format(
                    "sum(descendant::nsBooks:book[@nsGenre:genre='{0}' and nsBooks:price < {1}]/descendant::nsBooks:price) div count(descendant::nsBooks:book[@nsGenre:genre='{0}' and nsBooks:price < {1}]/descendant::nsBooks:price)",
                    genre, decimal.Parse(price));

            var averagePrice = (double)_gBookStoreXdoc.CreateNavigator().Evaluate(avgPriceCalcExpr, _gNsMngr);
            Console.WriteLine("Average price \t{0}", averagePrice);
        }

        [Action]
        public static void SaveBook(
            [Required] string genre,
            [Required] string publicationDate,
            [Required] string isbn,
            [Required] string title,
            [Required] string authorFirstName,
            [Required] string authorLastName,
            [Required] string price
            )
        {
            Console.WriteLine("---------------------\nSaving a new book \n---------------");

            XmlNode book = _gBookStoreXdoc.DocumentElement.FirstChild;
            book.Attributes["genre", _gNsMngr.LookupNamespace("nsGenre")].Value = genre;
            book.Attributes["ISBN"].Value = isbn;
            book.Attributes["publicationdate"].Value = publicationDate;
            book.SelectSingleNode("descendant::nsBooks:title", _gNsMngr).InnerText = title;
            book.SelectSingleNode("descendant::nsAuthors:author/descendant::nsAuthors:first-name", _gNsMngr).InnerText = authorFirstName;
            book.SelectSingleNode("descendant::nsAuthors:author/descendant::nsAuthors:last-name", _gNsMngr).InnerText = authorLastName;
            book.SelectSingleNode("descendant::nsBooks:price", _gNsMngr).InnerText = price;
            _gBookStoreXdoc.DocumentElement.AppendChild(book);
            _gBookStoreXdoc.Save(XmlFilePath);

            Console.WriteLine("book saved....\n");
            PrintBook(book);
        }

        private static string GetAuthorName(XmlNode book)
        {
            return string.Format("{0} {1}",
                                 book.SelectSingleNode(
                                     "descendant::nsAuthors:author/descendant::nsAuthors:first-name", _gNsMngr).
                                     InnerText,
                                 book.SelectSingleNode("descendant::nsAuthors:author/descendant::nsAuthors:last-name",
                                                       _gNsMngr).InnerText);
        }

        private static void PrintBook(XmlNode book)
        {
            Console.WriteLine("Title \t{0}", book.SelectSingleNode("descendant::nsBooks:title", _gNsMngr).InnerText);
            Console.WriteLine("Author \t{0}", GetAuthorName(book));
            Console.WriteLine("Genre \t{0}", book.Attributes["genre", _gNsMngr.LookupNamespace("nsGenre")].Value);
            Console.WriteLine("Price \t${0}", book.SelectSingleNode("descendant::nsBooks:price", _gNsMngr).InnerText);
            Console.WriteLine("ISBN \t{0}", book.Attributes["ISBN"].Value);
            Console.WriteLine("Date \t{0}", book.Attributes["publicationdate"].Value);
            Console.WriteLine("\n");
        }
    }
}