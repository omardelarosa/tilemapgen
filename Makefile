TilemapGen.exe:
	csc TilemapGen.cs

start: clean TilemapGen.exe
	mono TilemapGen.exe

clean:
	bin/clean