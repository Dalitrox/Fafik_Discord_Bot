using Discord.WebSocket;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fafik.Utilities
{
    public class Images
    {
        public async Task<string> CreateImageAsync(SocketGuildUser user, string url = "https://images.unsplash.com/photo-1419242902214-272b3f66ee7a?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1213&q=80") //utworzone jest zadanie, w którym wartością zwracaną jest string (ścieżka) a jako
                                                                                                                                                                                                                                                        //parametr jest SocketGuildUser czyli użytkownik serwera pod zmienną user
        {
            var avatar = await FetchImageAsync(user.GetAvatarUrl(size: 2048, format: Discord.ImageFormat.Png) ?? user.GetDefaultAvatarUrl()); //pobiera zdjęcie profilowe użytkownika i najlepszej
                                                                                                                                              //jakości 2048px i w formacie PNG, który nadaje się
                                                                                                                                              //najlepiej bo jest transparentny, a jeśli użytkownika
                                                                                                                                              //nie ma zdjęcia profilowego (własnego) to wtedy wyświetli
                                                                                                                                              //domyślne zdjęcie dla discorda
            var background = await FetchImageAsync(url);

            background = CropToBanner(background); //przycięcie tła do formatu banneru

            avatar = ClipImageToCyrcle(avatar); //wstawienie zdjęcia profilowego jako okrągłe zdjęcie

            var bitmap = avatar as Bitmap; //stąd jest brana bitmapa avatara
            bitmap?.MakeTransparent(); //jeśli nie jest nullem to zrób je transparentne

            var banner = CopyRegionIntoImage(bitmap, background); //bitmap jest to bitmapa avatara i tło

            banner = DrawTextToImage(banner, $"{user.Username}#{user.Discriminator} dołączył do serwera!", $"Członek serwera {user.Guild.Name}!");

            string path = $"{Guid.NewGuid()}.png";

            banner.Save(path);

            return await Task.FromResult(path);
        }

        private static Bitmap CropToBanner(Image image) //ta funkcja będzie przycinała zdjęcie tła tak żeby powstał banner
        {
            var originalWidth = image.Width; //zapisanie w tej zmiennej szerokości zdjęcia tła
            var originalHeight = image.Height; //zapisanie w tej zmiennej wysokości zdjęcia tła

            var destinationSize = new Size(1100, 450); //zmienna, która zawiera proporcje, które chcemy otrzymać (szerokość, wysokość)

            var heightRatio = (float)originalHeight / destinationSize.Height; //określi stosunek zdjęcia po przez podzielenie oryginalnej wysokości przez tą którą zamierzamy osiągnąć
            var widthRatio = (float)originalWidth / destinationSize.Width; //określi stosunek zdjęcia po przez podzielenie oryginalnej szerokości przez tą którą zamierzamy osiągnąć

            var ratio = Math.Min(heightRatio, widthRatio); //weźmie najmniejsze wartości z tych dwóch zmiennych jakie są możliwe

            var heightScale = Convert.ToInt32(destinationSize.Height * ratio); //określa skalowanie wysokości zdjęcia
            var widthScale = Convert.ToInt32(destinationSize.Width * ratio); //określa skalowanie szerokości zdjęcia

            var startX = (originalWidth - widthScale) / 2;
            var startY = (originalHeight - heightScale) / 2;

            var sourceRectangle = new Rectangle(startX, startY, widthScale, heightScale); //stworzenie prostokąta na bazie oryginalnych wymiarów
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height); //stworzenie bitmapy bazowany na wymiarach jakie chcemy otrzymać
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height); //stworzenie prostokąta na bazie bitmapy z wymiarami jakie zamierzamy osiągnąć

            using var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; //chcemy otrzymać przerobione zdjęcie w miarę możliwości najwyższej jakości
            g.DrawImage(image, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);

            return bitmap;
        }

        private Image ClipImageToCyrcle(Image image)
        {
            Image destination = new Bitmap(image.Width, image.Height, image.PixelFormat); //stworzenie nowej bitmapy zdjęcia

            var radious = image.Width / 2; //promień okręgu jest ilorazem szerokości zdjęcia profilowego na 2

            var x = image.Width / 2; //x i y wyznaczają środek tego okręgu (do zdjęcia)
            var y = image.Height / 2;

            using Graphics g = Graphics.FromImage(destination);

            var r = new Rectangle(x - radious, y - radious, radious * 2, radious * 2); //utworzenie nowego prostokąta z x - promień okręgu, y - promień okręgu oraz wysokość i szerokość równe promieniowi
                                                                                       //pomnożone razy 2

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality; //wszystkie te 4 ustawienia są po to żeby uzyskać jak najlepszą (możliwie) jakość zdjęcia
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(Color.Transparent)) //utowrzenie nowego pędzla, który jest transparentny (przezroczysty)
            {
                g.FillRectangle(brush, 0, 0, destination.Width, destination.Height); //utworzenie prostokąta wypełnionym przezroczystym kolorem
            }

            var path = new GraphicsPath();
            path.AddEllipse(r); //wstawienie zdjęcia w przezroczysty prostokąt, który zostaje przekształcony w okrąg

            g.SetClip(path);
            g.DrawImage(image, 0, 0);

            return destination;
        }

        private Image CopyRegionIntoImage(Image source, Image destination) //source = avatar (zdjęcie profilowe) i destination = background (tło)
        {
            using var grD = Graphics.FromImage(destination);
            var x = (destination.Width / 2) - 110; //ułożenie zdjęcia dosłonie na środku banneru
            var y = (destination.Height / 2) - 155;

            grD.DrawImage(source, x, y, 220, 220); //ustawienie spurce - zdjęcia profilowego na pozycji x i Y (na środku banneru) z wymiarami 220px x 220px

            return destination;
        }

        private Image DrawTextToImage(Image image, string header, string subheader)
        {
            var planewalker = new Font("Planewalker", 30, FontStyle.Regular); //główny duży napis
            var planewalkerSmall = new Font("Planewalker", 23, FontStyle.Regular); //mniejszy napis

            var brushWhite = new SolidBrush(Color.White); //kolor głównego napisu
            var brushGray = new SolidBrush(ColorTranslator.FromHtml("#3ce3e6")); //kolor mniejszego napisu w kodzie szenstastkowym

            var headerX = image.Width / 2;
            var headerY = (image.Height / 2) + 115;

            var subheaderX = image.Width / 2;
            var subheaderY = (image.Height / 2) + 160;

            var drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center, //zamiast zgadywać i odnajdywać centrum banneru dla napisu StringAligment.Center zrobi to automatycznie
                Alignment = StringAlignment.Center
            };

            using var GrD = Graphics.FromImage(image);
            GrD.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            GrD.DrawString(header, planewalker, brushWhite, headerX, headerY, drawFormat);
            GrD.DrawString(subheader, planewalkerSmall, brushGray, subheaderX, subheaderY, drawFormat);

            var img = new Bitmap(image);

            return image;
        }

        private async Task<Image> FetchImageAsync(string url) //to prywatne zadane ma zwracać zdjęcie gdzie parametrem jest string z URL zdjęcia
        {
            var client = new HttpClient(); //tworzy nowego klienta http
            var response = await client.GetAsync(url); //pobiera link URL zdjęcia od użytkownika

            if (!response.IsSuccessStatusCode) //jeśli nie będzie w stanie znaleźć tego linku wtedy generuje przy użyciu domyślnego link do zdjęcia
            {
                var backupResponse = await client.GetAsync("https://images.unsplash.com/photo-1419242902214-272b3f66ee7a?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1213&q=80");
                var backupStream = await backupResponse.Content.ReadAsStreamAsync(); //konwertuje link na stream, który potem zwróci jako zdjęcie
                return Image.FromStream(backupStream); //zwraca zdjęcie
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }
    }
}
