public class Consts {
	public struct Physics {
		public const float GRAVITY = -9.81f;
	}

	public struct Tags {
		public const string PLAYER = "Player";
		public const string INTERACT = "Interact";
	}

	public struct Anims {
		public const string SPEED = "Speed";
		public const string OPEN = "Open";
		public const string ON = "On";
	}

	public struct Menu {
		public const string MAINMENU_LEVEL_NAME = "MainMenu";
		public const string LOAD_LEVEL_NAME = "Load";
		public const string MAIN_LEVEL_NAME = "Main";
	}

	public struct Hints {
		public const string INTERACT_HINT = "Press 'E' to interact with ";
		public const string DOOR_LOCKED = "Don't have the key";
		public const string CANT_ESCAPE_YET = "Can't escape yet, must find the book";
	}

	public struct Quests {
		public const string INITIAL_QUEST = "Find the Book of Death";
		public const string BOOK_OF_DEATH = "Book of Death";
		public const string ESCAPE_QUEST = "ESCAPE";
	}
}
