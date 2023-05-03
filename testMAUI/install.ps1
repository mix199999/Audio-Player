#################################################################################################################
# Po kolei                                                                                                      #
#   1. Stworzyc folder jakkolwiek nazwany np.: "GNOM_installation" czy dowolnie inaczej                         #
#   2. Stworzyc paczke z instalatorem .msix, uzywajac Project "GNOME" -> "Publish" z poziomu Visual Studio      #
#   3. Wziac paczke (z punktu 2.) i wrzucic ja do folderu z 1.                                                  #
#   4. Zmienic nazwe paczki na "data"                                                                           #
#   5. Uzyc programu PS2EXE aby zrobic .exe z tego skryptu (install.ps1)                                        #
#   6. Wrzucic install.exe do folderu z punktu 1.                                                               #
#   7. Wrzucic instrukcje do folderu data                                                                       #
#   Na koniec struktura powinna wygladac tak:                                                                   #
#                                                                                                               #
#   GNOM_installation |                                                                                         #
#                     |--- install.exe                                                                          #
#                     |--- data |                                                                               #
#                               |--- <plik z certyfikatem>                                                      #
#                               |--- <plik .msix>                                                               #
#                               |--- instrukcja.pdf                                                             #
#                               |--- itp. itd.                                                                  #
#                                                                                                               #
#################################################################################################################

# Installation script for GNOM Audio-Player project

$data_path = ".\data"

# Remember current location of the Powershell
$curr_loc = Get-Location
Set-Location $PSScriptRoot

# Check if "data" folder exists
if (-not (Test-Path -Path $data_path)) {
    # Display error
    [System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms')
    [System.Windows.Forms.MessageBox]::Show('Could not find "data" folder with installation files. The "data" folder should be in the same folder as this (install.ps1) script!','ERROR')

    Set-Location $curr_loc
    return
}

# Maybe check for all needed contents, like Install.ps1, the folders, .msix etc?
# TODO

# Install certificate
$cert_path =  Get-ChildItem -Path $data_path -Filter *.cer -File -Name
Import-Certificate -FilePath $data_path\$cert_path -CertStoreLocation Cert:\LocalMachine\Root

# Run .msix installer
$gui_installer_path = Get-ChildItem -Path $data_path -Filter *.msix -File -Name
Invoke-Expression $data_path\$gui_installer_path

# Copy the instruction
$instruction_path = $data_path + "\instrukcja.pdf"
if (Test-Path -Path $instruction_path -PathType Leaf) {
    $aux_path = [Environment]::GetFolderPath("MyDocuments")
    $aux_path += "\GNOM"
    Copy-Item $instruction_path -Destination $aux_path
}

Set-Location $curr_loc