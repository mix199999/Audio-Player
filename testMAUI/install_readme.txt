Instalator musi być wywołany:
- albo jako administrator
- albo przez użytkownika który ma uprawnienia administratora na danym komputerze
inaczej program "install.exe" nie zadziała poprawnie.

!! WAŻNE !!
Aby zainstalować program trzeba zainstalować certyfikat wygenerowany przez twórcow tego
programu do folderu certyfikatow "root".
Jeżeli Państwo nie chcą ryzykowac instalowania nieznanego certyfikatu (co jest dobrą
praktyką) to niestety nie zainstalują Państwo aplikacji. 
Wszelkie zażalenia i groźby proszę kierować do Microsoft'u, gdyż to oni w swoim geniuszu
na taki sposób nas zmusili :).

Jeżeli nie chcą Państwo używac tego skryptu (install.exe) można zainstalować program "GNOM" 
manualnie. Tutaj wypisano kroki które trzeba wykonać aby zainstalować program maualnie:
	1. Zainstalować certyfikat z folderu "data" który kończy się na ".cer"
	2. Zainstalować program poprzez wywołanie programu .msix z folderu "data"
	3. Stworzenie folderu "GNOM" w folderze specjalnym "Dokumenty"
	4. Skopiowanie pliku instrukcja.pdf do stworzonego folderu GNOM w folderze specjalnym Dokumenty