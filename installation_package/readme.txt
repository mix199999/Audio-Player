Readme do pakietu instalacyjnego

!! WAŻNE !!
Aby zainstalować program trzeba zainstalować certyfikat wygenerowany przez twórcow tego
programu ("nas", sekcja "B - Odtwarzacz") do folderu certyfikatow "root".
Jeżeli Państwo nie chcą ryzykowac instalowania nieznanego certyfikatu (co jest dobrą
praktyką) to niestety nie zainstalują Państwo aplikacji. 
Po odinstalowaniu aplikacji GNOM prosimy o odinstalowanie naszego certyfikatu o nazwie:
"GNOM_cert"
Wszelkie zażalenia i groźby proszę kierować do Microsoft'u, gdyż to oni w swoim geniuszu
na taki sposób nas zmusili :).

Instalator install.exe musi być wywołany:
- albo jako administrator
- albo przez użytkownika który ma uprawnienia administratora na danym komputerze
inaczej program "install.exe" nie zadziała poprawnie.

Jeżeli install.exe zostanie "wykryty" jako wirus można:
- uruchomić "PowerShell" jako administrator i wykonać w powershell'u skrypt install.ps1
- zainstalować manualnie program (rozpisano co trzeba zrobić poniżej)
- zbudować manualnie program w Visual Studio (wymaga .NET 7.0 i bibliotek podanych w dokumentacji - powinny sie automatycznie instalować przez NuGet'a)
- nie instalować programu

Jeżeli nie chcą Państwo używać tego skryptu (install.exe) można zainstalować program "GNOM" 
manualnie. Tutaj wypisano kroki które trzeba wykonać aby zainstalować program maualnie:
	1. Zainstalować certyfikat z folderu "data" który kończy się rozszerzeniem ".cer"
	(należy go zainstalować w fodlerze ROOT certyfikatów, najlepiej dla całej maszyny lokalnej)
	2. Zainstalować program poprzez wywołanie programu z rozszerzeniem .msix z folderu "data"
	3. Stworzenie folderu "GNOM" w folderze specjalnym "Dokumenty"
	4. Skopiowanie pliku instrukcja.pdf do stworzonego folderu GNOM w folderze specjalnym Dokumenty

Jeżeli przy instalowaniu programu (przy użyciu .msix) wyświetli się coś w stylu "nie można zweryfikować
certyfikatu" czy coś podobnego, to albo certyfikat się nie zainstalował albo zainstalowano go w złym
folderze certyfikatów