@echo off
cls
echo/
echo //--------------------------------------------------------------------------------------
echo // CREATE MIGRATIONS SCRIPT
echo //
echo // Required input:
echo // --------------
echo // Project name   : The project that contains the DbContext
echo // DbContext name : The DbContext for the migration
echo // Migration name : This script will add the "Migration_<dbContext>" suffix to the name
echo //--------------------------------------------------------------------------------------
echo/
set /p   project="Project name (eShopDashboard) : " || set project=eShopDashboard
set /p dbContext="DbContext name                : "
set /p      name="Migration name                : "

set scriptsDir=%cd%
set cliProjectDir=.\Scripts.Cli
set migrationsPath=Infrastructure\Migrations
set migrationsFolder=%dbContext:DbContext=%
set migrationsFolder=%migrationsFolder:Context=%

@echo cd %cliProjectDir%
cd %cliProjectDir%

@echo on
dotnet ef migrations add %name%Migration_%dbContext% -p ..\..\src\%project% -c %dbContext% -o ..\..\src\%project%\%migrationsPath%\%migrationsFolder%
@echo off

cd %scriptsDir%
