Gałęzie każdemu zrobie jak skończe robić set-up repo. Jak checie to możeci sobie wtedy zmienić nazwe swojej własnej gałęzi xd

# Audio-Player
Poniżej zostaną przedstawione proponowane przeze mnie zasady dot. części bardziej "implementacyjnej" projektu żeby nikt nikomu sobie na głowe nie wchodził i żeby wszystko przebiegało w miarę sprawnie.

## Użytkowanie repo
- Każdy z Was będzie miał swoją gałąź (branch'a) na której będzie pracował, + będzie gałąź główna.
- Propozycja "workflow" jest następująca:
  - Zanim zaczniecie pracę nad swoimi "modułami" (nwm jak inaczej nazwać te zmiany które będziecie dodawać do projektu xd) aktualizujecie swoje lokalne repo (albo po prostu swoją gałąź poprzez Pull Request'a online) na podstawie tego repo online, żeby mieć jak najnowszą wersje.
  - Robicie swoje zmiany lokalnie
  - Jak uznacie że zrobilisćie wystarczająco dużo rzeczy to push'ujecie zmiany na swoją gałąź online z lokalnej, i robicie PR ze swojej gałęzi na gałąź "main"
- Na waszych branch'ach, jak i u Was lokalnie możecie robić sobie co checie, testować jak checie, to nie ma znaczenia. Ilość commitów i  w sumie tytuły commit'ów też nie są do końca ważne.
- Taki "workflow" proponuje żeby na gałęzi głównej "main" był TYLKO kod który działa, tj. żeby wszystko co jest na main'ie dało się zbudować, a nie że ktoś wrzuci na main'a coś co sie nie buduje, potem druga osoba zobaczy że są zmiany, pobierze je do siebie lokalnie lub na swoja gałąź, spróboje zbudować projekt i sie okaże że musi czekać z pracą aż oryginalny autor zmian poprawi te swoje zmiany bo sie projekt nie buduje
- PR do main'a: Jak nazwy i ilość commit'ów to w sumie wasza sprawa tylko, to prosiłbym żeby w każdym PR który będzie robiony aby zintegrować Wasze zmiany na gałąź "main", napisaliście w opisie tego pull request'a co tam dodaliście / zmieniliście itp., bo ja musze robić dokumentacje z tego projektu i taki opis zmian na pull request'ach bardzo by mi pracę ułatwił bo inaczej bym musiał sie za każdym razem Was pytać co to za zmiany poszły, albo musiałbym kod analizować xd

## Korzystanie z zewnętrznych bibliotek / zasobów
Jeżeli będziecie korzystać z bibliotek / zasobów (plików muzycznych) / czegokolwiek co zintegrujecie "do środka projektu" albo co pójdzie do repozytorium to **proszę Was bardzo** żebyście dodali ten "zasób" do listy poniżej, bo wszystkie zasoby i narzędzia muszą zostać opisane skąd pochodzą i jakie mają licensje. Co do licensji itp to ja już moge sam to ogarnać i znaleźć informacje ale potrzebuje info co do tego:
- czego używaliście
- skąd to wzieliście

# Lista bibliotek / narzędzi / zasobów
- .NET framework, https://dotnet.microsoft.com/ - MIT License
- .NET MAUI, https://dotnet.microsoft.com/en-us/apps/maui - MIT License
- Blazor, (część ASP.NET core, pisze ze jest Apache 2.0 ale ASP.NET to jest MIT wiec wtf?), Apache 2.0
- <zasób>, <skąd to wzieliście>>
