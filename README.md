# Audio-Player
Desktop "Audio Player" type of an app for a university project

## Co jest zrobione

Update #5
- prototyp panelu do robienia nowych playlist
- Zmieniono nazwe programu z "testMAUI na "GNOM"
- dodano ikonke gnoma jako ikonke programu
- zmieniono ścieżkę appSettings.json do ...\Documents\GNOM\appSettings.json
- zmieniono ścieżkę favoritesongs.m3u do ...\Documents\GNOM\favoritesongs.m3u
- usunieto BUG ze pierwsza dodana piosenka do playlisty ulubionych sie nie dodawala poprawnie
- dodano do projektu skrypt instalujacy certyfikat + apke + instrukcje
- odpalanie instrukcji przy pierwszym starcie
- przycisk w Ustawieniach do odpalania instrukcji

Update #4
- Wyszukiwarka
  - Naciśnięcie Enter/ikony lupy powoduje załadowanie wszystkich utworów z listy znalezionych utworów
  - wybranie pojedyńczego elementu z listy znalezionych utworów ładuje go do playlisty
  
Update #3
- Poprawiona optymalizacja strony z ustawieniami 
  - teraz właściwie tworzy się w innym wątku (znacznie szybciej działa)
- Automatyczne tworzenie się playlisty ulubione utwory + dodanie jej do pliku konfiguracyjnego:
    - AppendTrackToFavoritelistFile(AudioFile track) - metoda pozwala na dopisywanie do playlisty w formacie .m3u kolejnych utworów
    - RemoveTrackFromM3U(AudioFile track) - odwrotna metoda do AppendTrackToPlaylist
    
- Przebudowanie metod do wyświetlania w ListView:
  - Utworzono model danych dla ListView z utworami (coś jak model dla adaptera w javie)
  - Zmodyfikowano metodę na potrzeby wyświetlania czy utwór w danym zbiorze znajduje się w playliscie ulubione
  
- Zmiana eventów z selected item changed na itemtapped:
  - Wynikało to z problemu z wczytywaniem się playlist z listy playlist
  - Jedyny sposób na obsłużenie polubienia tzn. double-click na element listy powoduje like/unlike
  
- Obsłużenie powrotu do playlisty zawierającej utwory z wybranych ścieżek
- Obsłużenie usuwania folderów z listy w ustawieniach
- ...i wiele mniejszych błędów poprawiono...

Update #2
- startowa playlista
- wybieranie folderów do domyślnej playlisty
- systemowa notyfikacja z informacjami utworu (gdy program jest w tle)
- plik konfiguracyjny(na razie samo zapisywanie wybranych "ulubionych" folderów)

Update #1
- sterowanie odtwarzaczem
- dodawanie utworów do playlist
- tworzenie/wczytywanie playlist w formacie .M3U
- pobieranie informacji o utworze wykorzystując bibliotekę TagLib
- pobieranie okładki albumu z api deezer (jeśli jest to możliwe)
- wydzielenie poszczególnych segmentów na ekranie startowym


### BUGFIX 1.0

- dodano obsługę błedów przy dodawaniu utworów/playlist nie zawierających informacji w metadanych tj. nazwa utworu
- sprawdzanie czy plik istnieje pod zapisaną lokalizacją we wczytywanej playliscie 


## Link do użytych repozytoriów

#### TagLib#

Link: https://github.com/mono/taglib-sharp

TagLib# is a library for reading and editing metadata in various media files, including MP3, Ogg Vorbis, and Windows Media Audio. It is written in C# and provides a convenient object-oriented API for accessing media file metadata.

#### Microsoft.Maui.CommunityToolkit

Link: https://github.com/CommunityToolkit/Maui

Microsoft.Maui.CommunityToolkit is a collection of useful controls, helpers, and extensions for building apps with .NET Multi-platform App UI (MAUI). It provides a set of cross-platform controls and services that can be used to simplify app development across different platforms.

#### NAudio

Link: https://github.com/naudio/NAudio

NAudio is a library for working with audio in .NET applications. It supports playback, recording, and conversion of various audio formats, including MP3, WAV, and AAC. It also provides a set of useful features, such as audio mixing and effects processing.

#### Newtonsoft.Json

Link: https://www.newtonsoft.com/json

Newtonsoft.Json is a popular JSON framework for .NET applications. It provides a fast and flexible API for working with JSON data, including serialization and deserialization of objects, LINQ support, and schema validation. It is widely used in many .NET projects, including ASP.NET, Xamarin, and Unity.
