TilemapGen.exe:
	csc TilemapGen.cs

start: TilemapGen.exe
	mono TilemapGen.exe

clean:
	rm TilemapGen.exe