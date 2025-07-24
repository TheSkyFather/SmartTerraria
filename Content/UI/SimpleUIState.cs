using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using ReLogic.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using SmartTerraria.Content.Items.Armor;
using Terraria.Localization;

namespace SmartTerraria
{
    // Класс системы пользовательского интерфейса
    public class SimpleUISystem : ModSystem
    {
        // Интерфейс пользователя
        private UserInterface _interface;
        // Состояние пользовательского интерфейса
        public SimpleUIState _uiState;

        // Горячая клавиша для переключения UI
        public static ModKeybind SimpleKeybind { get; private set; }

        // Метод, вызываемый при загрузке мода
        public override void Load()
        {
            if (!Main.dedServ)
            {
                // Инициализируем состояние UI
                _uiState = new SimpleUIState();
                _uiState.Activate();

                // Создаём новый пользовательский интерфейс и устанавливаем его состояние
                _interface = new UserInterface();
                _interface.SetState(_uiState);

                // Регистрируем горячую клавишу для переключения UI
                SimpleKeybind = KeybindLoader.RegisterKeybind(Mod, "toggleUI", "P");
            }
        }

        // Модифицируем слои интерфейса игры
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

            if (inventoryLayerIndex != -1)
            {
                layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
                    "SmartTerraria: UI",
                    delegate
                    {
                        if (!Main.gameMenu && _uiState.Visible)
                        {
                            _interface.Update(Main._drawInterfaceGameTime);
                            _interface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }

        // Свойство для доступа к состоянию UI
        public SimpleUIState UIState => _uiState;
    }

    // Новый класс для выравнивания текста по левому краю
    public class LeftAlignedText : UIText
    {
        private float _textScale;

        public LeftAlignedText(string text, float textScale = 1f, bool large = false) : base(text, textScale, large)
        {
            _textScale = textScale;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            // Получаем размеры элемента
            CalculatedStyle innerDimensions = GetInnerDimensions();
            Vector2 position = innerDimensions.Position();

            // Цвет текста
            Color color = IsMouseHovering ? Color.Yellow : Color.White;

            // Рисуем текст с выравниванием по левому краю
            Utils.DrawBorderString(spriteBatch, Text, position, color, _textScale, 0f, 0f);
        }
    }

    // Класс состояния пользовательского интерфейса
    public class SimpleUIState : UIState
    {
        // Элементы UI
        private UIPanel _panel;
        private UIPanel _headerBackground;
        private UIScrollbar _scrollbar;
        private UIText _headerText;
        private UITextPanel<string> _closeButton;

        private bool _resizing;
        private Vector2 _resizeOffset;
        private bool _dragging;
        private Vector2 _dragOffset;

        // Элементы для отображения статистики урона
        private UIText _meleeDamageText;
        private UIText _rangedDamageText;
        private UIText _magicDamageText;
        private UIText _summonDamageText;
		private UIText _baseBonus;
        private UIText _additionalBonus;
        private UIText _bossesKilled;
		private UIText _shine;
		private UIText _spelunker;
		private UIText _mining;
		private UIText _defenseStatHelm;
		private UIText _highestClassText;
		private UIText _immuneFire;
		private UIPanel _descriptionPanel;
		private string _currentDescriptionText = "";
		private float _lineHeight;
		private List<string> _descriptionLines;

		private bool _textChanged = false; // Флаг, что текст обновился
		private bool _effectTextChanged = false; // флаг для отслеживания изменений в тексте блока эффектов

		private DynamicSpriteFont _font;
		private float _scale = 1f;
		
		
		private UIPanel _effectStatsPanel;
		private UIPanel _effectHeaderBackground;
		private UIText _effectHeader;
		private List<string> _effectStatsLines = new List<string>();

		private string _shineText = "";
		private string _miningText = "";
		private string _spelunkerText = "";


        // Свойство для управления видимостью UI
        public bool Visible { get; private set; } = true;

        // Метод инициализации UI
        public override void OnInitialize()
        {
            // Создаём основную панель с более тёмным цветом и прозрачностью
            _panel = new UIPanel();
            _panel.Width.Set(550f, 0f);
            _panel.Height.Set(600f, 0f);
            _panel.HAlign = 0.5f;
            _panel.VAlign = 0.5f;
            _panel.BackgroundColor = new Color(33, 43, 79, 200); // Более тёмный цвет с прозрачностью
            Append(_panel);

            // Создаём фон для заголовка основной панели
            _headerBackground = new UIPanel();
            _headerBackground.Width.Set(-40f, 1f);
            _headerBackground.Height.Set(40f, 0f);
            _headerBackground.BackgroundColor = new Color(44, 53, 101, 200); // Более тёмный цвет с прозрачностью
            _panel.Append(_headerBackground);

            // Создаём заголовок основной панели
			_headerText = new UIText(Language.GetText("Mods.SmartTerraria.SimpleUIState.Header"));

            _headerText.HAlign = 0f; // Выравнивание по левому краю
            _headerText.VAlign = 0.5f;
            _headerText.Left.Set(10f, 0f); // Отступ слева
			_headerText.TextColor = new Color(0x00, 0xFF, 0x00);
            _headerBackground.Append(_headerText);

            // Создаём кнопку закрытия
            _closeButton = new UITextPanel<string>("X");
            _closeButton.Width.Set(40f, 0f);
            _closeButton.Height.Set(40f, 0f);
            _closeButton.HAlign = 1f; // Выравнивание по правому краю основной панели
            _closeButton.VAlign = 0f; // Выравнивание по верхнему краю
            _closeButton.BackgroundColor = _headerBackground.BackgroundColor; // Такой же цвет, как у заголовка
            _closeButton.BorderColor = _headerBackground.BorderColor;
            _closeButton.TextColor = new Color(255, 100, 100); // Текст 'X' чуть светлее
            _closeButton.OnLeftClick += CloseButtonClicked;
            _panel.Append(_closeButton);

            // Создаём полосу прокрутки
            _scrollbar = new UIScrollbar();
            _scrollbar.Width.Set(20f, 0f);
            _scrollbar.Height.Set(-50f, 1f);
            _scrollbar.Top.Set(50f, 0f);
            _scrollbar.HAlign = 1f;
            _panel.Append(_scrollbar);

            // Создаём список содержимого
            UIList contentList = new UIList();
            contentList.Width.Set(-25f, 1f);
            contentList.Height.Set(-50f, 1f);
            contentList.Top.Set(50f, 0f);
            contentList.ListPadding = 10f;
            contentList.SetScrollbar(_scrollbar);
            _panel.Append(contentList);

            // Создаём и добавляем блоки в список содержимого
            UIPanel descriptionPanel = CreateDescriptionPanel();
            contentList.Add(descriptionPanel);
			
			// Создаём и добавляем блок "Статистика бонусов"
            UIPanel bonusStatsPanel = CreateStatPanel();
            contentList.Add(bonusStatsPanel);
			
            // Создаём и добавляем блок "Статистика урона"
            UIPanel damageStatsPanel = CreateDamageStatsPanel();
            contentList.Add(damageStatsPanel);

            // Создаём и добавляем блок "Статистика эффектов"
            UIPanel effectStatsPanel = CreateEffectStatsPanel();
            contentList.Add(effectStatsPanel);
			
			UIPanel immunePanel = CreateImmunePanel();
            contentList.Add(immunePanel);
        }

        // Обработчик нажатия на кнопку закрытия
        private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            ToggleVisibility();
        }
		
		private void RebuildDescriptionText()
		{
			// Удаляем старые строки (кроме _defenseStatHelm и header)
			List<UIElement> toRemove = new List<UIElement>();
			foreach (var element in _descriptionPanel.Children)
			{
				if (element is UIText utext && utext != _defenseStatHelm && utext.Text != Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.Header"))
				{
					toRemove.Add(element);
				}
			}
			foreach (var rem in toRemove)
			{
				rem.Remove();
			}

			// Пересчитываем размеры панели
			_descriptionPanel.Recalculate();

			float availableWidth = _descriptionPanel.GetInnerDimensions().Width - 20f;
			_descriptionLines = WrapText(_currentDescriptionText, availableWidth);

			float currentY = 50f;

			foreach (string line in _descriptionLines)
			{
				UIText lineText = new UIText(line, _scale);
				lineText.Top.Set(currentY, 0f);
				lineText.Left.Set(10f, 0f);
				_descriptionPanel.Append(lineText);
				currentY += _lineHeight;
			}

			// Перемещаем _defenseStatHelm ниже
			_defenseStatHelm.Top.Set(currentY + 0f, 0f);
			_defenseStatHelm.Left.Set(10f, 0f);

			// Устанавливаем высоту панели
			_descriptionPanel.Height.Set(currentY + _lineHeight + 15f, 0f);
			_descriptionPanel.Recalculate();
			Recalculate();
		}

		// Метod автопереноса
		private List<string> WrapText(string text, float maxWidth)
		{
			List<string> lines = new List<string>();
			string[] words = text.Split(' ');

			DynamicSpriteFont font = _font; 
			float spaceWidth = font.MeasureString(" ").X * _scale;

			string currentLine = "";
			foreach (string word in words)
			{
				string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
				
				float testLineWidth = font.MeasureString(testLine).X * _scale;
				if (testLineWidth <= maxWidth)
				{
					// Если слово умещается в текущую линию - добавляем его
					currentLine = testLine;
				}
				else
				{
					// Слово не помещается в текущую строку.
					// Проверяем, не слишком ли оно длинное само по себе.
					float wordWidth = font.MeasureString(word).X * _scale;
					if (wordWidth > maxWidth)
					{
						// Слово длиннее максимальной ширины. Разбиваем его на части.
						// Сначала, если в currentLine уже есть текст - добавим эту линию в итог.
						if (!string.IsNullOrEmpty(currentLine))
						{
							lines.Add(currentLine);
							currentLine = "";
						}

						string remainingWord = word;
						while (remainingWord.Length > 0)
						{
							int charCount = 0;
							float lineWidth = 0f;

							// Подбираем количество символов, которое влезет в строку
							while (charCount < remainingWord.Length)
							{
								float charWidth = font.MeasureString(remainingWord.Substring(charCount, 1)).X * _scale;
								if (lineWidth + charWidth > maxWidth)
									break;
								
								lineWidth += charWidth;
								charCount++;
							}

							// Добавляем строку из частей слова
							string part = remainingWord.Substring(0, charCount);
							lines.Add(part);
							remainingWord = remainingWord.Substring(charCount);
						}
					}
					else
					{
						// Слово само по себе влезает в строку, но не вместе с текущим текстом.
						// Добавляем текущую линию в список и начинаем новую с этим словом.
						if (!string.IsNullOrEmpty(currentLine))
						{
							lines.Add(currentLine);
						}
						currentLine = word;
					}
				}
			}

			// Добавляем последнюю строку, если в ней остался текст
			if (!string.IsNullOrEmpty(currentLine))
			{
				lines.Add(currentLine);
			}

			return lines;
		}


        // Метод для создания блока "Описание и характеристики"
        private UIPanel CreateDescriptionPanel()
		{
			UIPanel panel = new UIPanel();
			panel.Width.Set(0f, 1f);
			panel.SetPadding(10f);
			panel.BackgroundColor = new Color(33, 43, 79, 200);

			UIPanel headerBackground = new UIPanel();
			headerBackground.Width.Set(0f, 1f);
			headerBackground.Height.Set(40f, 0f);
			headerBackground.BackgroundColor = new Color(44, 53, 101, 200);
			panel.Append(headerBackground);

			UIText header = new UIText(Language.GetText("Mods.SmartTerraria.SimpleUIState.Description"));
			header.HAlign = 0f;
			header.VAlign = 0.5f;
			header.Left.Set(10f, 0f);
			header.TextColor = new Color(0x00, 0xFF, 0x00);
			headerBackground.Append(header);

			// Изначально текст пустой
			_currentDescriptionText = Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.DescriptionText");

			// Подсчитаем высоту строки
			_font = FontAssets.MouseText.Value;
			_lineHeight = _font.MeasureString(" ").Y * _scale;

			_descriptionPanel = panel;

			// Добавим _defenseStatHelm
			_defenseStatHelm = new UIText(Language.GetText("Mods.SmartTerraria.SimpleUIState.DefenseStatHelm") + " 5");
			panel.Append(_defenseStatHelm);

			// Вызовем метод, который отрисует текст и пересчитает высоту
			RebuildDescriptionText();

			return panel;
		}


        // Метод для создания блока "Статистика урона"
        private UIPanel CreateDamageStatsPanel()
        {
            UIPanel panel = new UIPanel();
            panel.Width.Set(0f, 1f);
            panel.SetPadding(10f);
            panel.BackgroundColor = new Color(33, 43, 79, 200);

            UIPanel headerBackground = new UIPanel();
            headerBackground.Width.Set(0f, 1f);
            headerBackground.Height.Set(40f, 0f);
            headerBackground.BackgroundColor = new Color(44, 53, 101, 200);
            panel.Append(headerBackground);

            UIText header = new UIText(Language.GetText("Mods.SmartTerraria.SimpleUIState.DamageStatsHeader"));
            header.HAlign = 0f;
            header.VAlign = 0.5f;
            header.Left.Set(10f, 0f);
            header.TextColor = new Color(0x00, 0xFF, 0x00);
            headerBackground.Append(header);

            // Создаём контейнеры для текста урона
            _meleeDamageText = new LeftAlignedText("[c/FFA500:Ближний бой:] 0");
            _meleeDamageText.Top.Set(50f, 0f);
            _meleeDamageText.Left.Set(10f, 0f);
            panel.Append(_meleeDamageText);

            _rangedDamageText = new LeftAlignedText("[c/FFA500:Дистанционный урон:] 0");
            _rangedDamageText.Top.Set(80f, 0f);
            _rangedDamageText.Left.Set(10f, 0f);
            panel.Append(_rangedDamageText);

            _magicDamageText = new LeftAlignedText("[c/FFA500:Магический урон:] 0");
            _magicDamageText.Top.Set(110f, 0f);
            _magicDamageText.Left.Set(10f, 0f);
            panel.Append(_magicDamageText);

            _summonDamageText = new LeftAlignedText("[c/FFA500:Призывной урон:] 0");
            _summonDamageText.Top.Set(140f, 0f);
            _summonDamageText.Left.Set(10f, 0f);
            panel.Append(_summonDamageText);

            panel.Height.Set(185f, 0f); // Устанавливаем высоту панели

            return panel;
        }
		
		private UIPanel CreateEffectStatsPanel()
		{
			UIPanel panel = new UIPanel();
			panel.Width.Set(0f, 1f);
			panel.SetPadding(10f);
			panel.BackgroundColor = new Color(33, 43, 79, 200);

			UIPanel headerBackground = new UIPanel();
			headerBackground.Width.Set(0f, 1f);
			headerBackground.Height.Set(40f, 0f);
			headerBackground.BackgroundColor = new Color(44, 53, 101, 200);
			panel.Append(headerBackground);

			UIText header = new UIText(Language.GetText("Mods.SmartTerraria.SimpleUIState.EffectStatsHeader"));
			header.HAlign = 0f;
			header.VAlign = 0.5f;
			header.Left.Set(10f, 0f);
			header.TextColor = new Color(0x00, 0xFF, 0x00);
			headerBackground.Append(header);

			// Сохраняем ссылки, чтобы можно было переотрисовать текст при изменении
			_effectStatsPanel = panel;
			_effectHeaderBackground = headerBackground;
			_effectHeader = header;

			// Изначально тексты пустые, они будут заданы в Update() с учётом условий
			// и после этого будет вызван RebuildEffectStatsPanel() для автопереноса.

			return panel;
		}

		// Метод для пересборки текста в панели эффектов
		private void RebuildEffectStatsPanel()
		{
			if (_effectStatsPanel == null) return;

			// Удаляем все старые текстовые элементы, кроме заголовка
			List<UIElement> toRemove = new List<UIElement>();
			foreach (var element in _effectStatsPanel.Children)
			{
				// Пропускаем шапку (headerBackground) и сам заголовок (header), удаляем всё остальное
				if (element != _effectHeaderBackground && element != _effectHeader)
				{
					toRemove.Add(element);
				}
			}

			foreach (var rem in toRemove)
			{
				rem.Remove();
			}

			_effectStatsPanel.Recalculate();
			float availableWidth = _effectStatsPanel.GetInnerDimensions().Width - 10f; // запас по ширине
			float currentY = 50f; // отступ снизу от заголовка

			// Список строк, которые мы хотим отобразить
			// В порядке: _shineText, затем _miningText, затем _spelunkerText
			var linesToDisplay = new List<string> { _shineText, _miningText, _spelunkerText };

			foreach (var textLine in linesToDisplay)
			{
				if (string.IsNullOrWhiteSpace(textLine))
					continue;

				// Переносим текст по словам
				var wrappedLines = WrapText(textLine, availableWidth);
				foreach (var line in wrappedLines)
				{
					UIText lineText = new UIText(line, _scale);
					lineText.Top.Set(currentY, 0f);
					lineText.Left.Set(10f, 0f);
					_effectStatsPanel.Append(lineText);
					currentY += _lineHeight;
				}

				// Добавим дополнительный отступ после каждого блока эффектов
				currentY += 10f;
			}

			// Пересчитываем высоту панели в зависимости от итогового количества строк
			float finalHeight = currentY + 10f; 
			_effectStatsPanel.Height.Set(finalHeight, 0f);
			_effectStatsPanel.Recalculate();
			Recalculate();
		}

		
		private UIPanel CreateImmunePanel()
        {
            UIPanel panel = new UIPanel();
            panel.Width.Set(0f, 1f);
            panel.SetPadding(10f);
            panel.BackgroundColor = new Color(33, 43, 79, 200);

            UIPanel headerBackground = new UIPanel();
            headerBackground.Width.Set(0f, 1f);
            headerBackground.Height.Set(40f, 0f);
            headerBackground.BackgroundColor = new Color(44, 53, 101, 200);
            panel.Append(headerBackground);

            UIText header = new UIText(Language.GetText("Mods.SmartTerraria.SimpleUIState.ImmuneStatsHeader"));
            header.HAlign = 0f;
            header.VAlign = 0.5f;
            header.Left.Set(10f, 0f);
            header.TextColor = new Color(0x00, 0xFF, 0x00);
            headerBackground.Append(header);

            panel.Height.Set(310f, 0f); // Устанавливаем высоту панели
            return panel;
        }
		
        // Метод для создания блоков статистики бонусов и эффектов
        private UIPanel CreateStatPanel()
        {
            UIPanel panel = new UIPanel();
            panel.Width.Set(0f, 1f);
            panel.SetPadding(10f);
            panel.BackgroundColor = new Color(33, 43, 79, 200); // Более тёмный цвет с прозрачностью

            UIPanel headerBackground = new UIPanel();
            headerBackground.Width.Set(0f, 1f);
            headerBackground.Height.Set(40f, 0f);
            headerBackground.BackgroundColor = new Color(44, 53, 101, 200); // Более тёмный цвет с прозрачностью
            panel.Append(headerBackground);

            UIText header = new UIText(Language.GetText("Mods.SmartTerraria.SimpleUIState.GameStatsHeader"));
            header.HAlign = 0f; // Выравнивание по левому краю
            header.VAlign = 0.5f;
            header.Left.Set(10f, 0f); // Отступ слева
            header.TextColor = new Color(0x00, 0xFF, 0x00);
            headerBackground.Append(header);
			
			_bossesKilled = new LeftAlignedText("[c/FFA500:Убито боссов:] 0");
            _bossesKilled.Top.Set(50f, 0f);
            _bossesKilled.Left.Set(10f, 0f);
            panel.Append(_bossesKilled);
			
            _baseBonus = new LeftAlignedText("[c/FFA500:Текущий базовый бонус:] 0");
            _baseBonus.Top.Set(80f, 0f);
            _baseBonus.Left.Set(10f, 0f);
            panel.Append(_baseBonus);

            _additionalBonus = new LeftAlignedText("[c/FFA500:Текущий дополнительный бонус:] 0");
            _additionalBonus.Top.Set(110f, 0f);
            _additionalBonus.Left.Set(10f, 0f);
            panel.Append(_additionalBonus);
			
			_highestClassText = new LeftAlignedText("[c/FFA500:Текущий доминирующий класс:] Нет");
            _highestClassText.Top.Set(140f, 0f);
            _highestClassText.Left.Set(10f, 0f);
            panel.Append(_highestClassText);
			
            panel.Height.Set(185f, 0f); // Устанавливаем высоту панели
            return panel;
        }

        // Метод обновления состояния UI
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Visible) return;
			
		
			
            // Обновляем значения урона
			
			float baseBonus = UniversalAdaptationHelmet.CurrentBaseBonus;
			float additionalBonus = UniversalAdaptationHelmet.CurrentAdditionalBonus;
            Player player = Main.player[Main.myPlayer];
            DamageTrackerPlayer damageTracker = player.GetModPlayer<DamageTrackerPlayer>();
			SmartTerrariaPlayer smartPlayer = player.GetModPlayer<SmartTerrariaPlayer>();
			DamageClass highestClass = smartPlayer.GetHighestDamageClass();
			
			if (smartPlayer.bossesKilled >= 4)
			{
				_shineText = $"[c/FFA500:{Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.ShineEffectHeader")}] {Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.ShineEffectDescription")}";
			}
			else
			{
				_shineText = $"[c/FFA500:{Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.ShineEffectHeader")}] {Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.EffectLocked")}";
			}

			if (smartPlayer.bossesKilled >= 6)
			{
				_miningText = $"[c/FFA500:{Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.MiningEffectHeader")}] {Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.MiningEffectDescription")}";
			}
			else
			{
				_miningText = $"[c/FFA500:{Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.MiningEffectHeader")}] {Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.EffectLocked")}";
			}

			if (smartPlayer.bossesKilled >= 8)
			{
				_spelunkerText = $"[c/FFA500:{Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.SpelunkerEffectHeader")}] {Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.SpelunkerEffectDescription")}";
			}
			else
			{
				_spelunkerText = $"[c/FFA500:{Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.SpelunkerEffectHeader")}] {Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.EffectLocked")}";
			}

			// Так как мы обновили текст, пересоберём панель
			_effectTextChanged = true;

			// Если произошли изменения, пересобираем панель
			if (_effectTextChanged)
			{
				RebuildEffectStatsPanel();
				_effectTextChanged = false;
			}

				
			string newText = Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.DescriptionText");
			if (newText != _currentDescriptionText)
			{
				_currentDescriptionText = newText;
				_textChanged = true;
			}
			if (_textChanged)
			{
				RebuildDescriptionText();
				_textChanged = false;
			}

			
            _meleeDamageText.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.MeleeDamage")}] {damageTracker.meleeDamageDealt}");
            _rangedDamageText.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.RangedDamage")}] {damageTracker.rangedDamageDealt}");
            _magicDamageText.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.MagicDamage")}] {damageTracker.magicDamageDealt}");
            _summonDamageText.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.SummonDamage")}] {damageTracker.summonDamageDealt}");
			_defenseStatHelm.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.DefenseStatHelm")}] {UniversalAdaptationHelmet.CurrentDefense}");
			
			string highestClassName = highestClass == DamageClass.Melee ? Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.MeleeClass")
                        : highestClass == DamageClass.Ranged ? Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.RangedClass")
                        : highestClass == DamageClass.Magic ? Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.MagicClass")
                        : highestClass == DamageClass.Summon ? Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.SummonClass")
                        : Language.GetTextValue("Mods.SmartTerraria.SimpleUIState.NotDefinedClass");

    // Обновляем текст в _highestClassText
			_highestClassText.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.HighestClass")}] {highestClassName}");


			
			
			_bossesKilled.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.BossesKilled")}] {smartPlayer.bossesKilled}");
			_baseBonus.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.BaseBonus")}] {baseBonus * 100:F2}%");
			_additionalBonus.SetText($"[c/FFA500:{Language.GetText("Mods.SmartTerraria.SimpleUIState.AdditionalBonus")}] {additionalBonus * 100:F2}%");
            // Обработка перетаскивания и изменения размера
            Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY);

            if (_headerBackground.ContainsPoint(mousePosition) || _dragging || _resizing)
            {
                Main.player[Main.myPlayer].mouseInterface = true;

                if (Main.mouseLeft && !_dragging && !_resizing)
                {
                    if (_headerBackground.ContainsPoint(mousePosition) && !_isOverResizeHandle(mousePosition))
                    {
                        _dragging = true;
                        _dragOffset = mousePosition - new Vector2(_panel.Left.Pixels, _panel.Top.Pixels);
                    }
                    else if (_isOverResizeHandle(mousePosition))
                    {
                        _resizing = true;
                        _resizeOffset = mousePosition - new Vector2(_panel.Width.Pixels, _panel.Height.Pixels);
                    }
                }

                if (!Main.mouseLeft)
                {
                    _dragging = false;
                    _resizing = false;
                }
            }

            if (_dragging)
            {
                _panel.Left.Set(mousePosition.X - _dragOffset.X, 0f);
                _panel.Top.Set(mousePosition.Y - _dragOffset.Y, 0f);
                Recalculate();
            }

            if (_resizing)
            {
                float newWidth = mousePosition.X - _resizeOffset.X;
                float newHeight = mousePosition.Y - _resizeOffset.Y;

                newWidth = MathHelper.Clamp(newWidth, 200f, 800f);
                newHeight = MathHelper.Clamp(newHeight, 150f, 800f);

                _panel.Width.Set(newWidth, 0f);
                _panel.Height.Set(newHeight, 0f);
                Recalculate();
            }
        }

        // Метод проверки, находится ли курсор над областью изменения размера
        private bool _isOverResizeHandle(Vector2 mousePosition)
        {
            const int resizeHandleSize = 16;
            Rectangle resizeRectangle = new Rectangle(
                (int)(_panel.Left.Pixels + _panel.Width.Pixels - resizeHandleSize),
                (int)(_panel.Top.Pixels + _panel.Height.Pixels - resizeHandleSize),
                resizeHandleSize,
                resizeHandleSize
            );

            return resizeRectangle.Contains(mousePosition.ToPoint());
        }

        // Метод переключения видимости UI
        public void ToggleVisibility()
        {
            Visible = !Visible;
            if (Visible)
            {
                Append(_panel);
                SoundEngine.PlaySound(SoundID.MenuOpen);
            }
            else
            {
                _panel?.Remove();
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }
    }

    // Класс игрока для обработки ввода
    public class SimplePlayer : ModPlayer
    {
        // Метод обработки триггеров ввода
        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            if (SimpleUISystem.SimpleKeybind.JustPressed)
            {
                var uiState = ModContent.GetInstance<SimpleUISystem>().UIState;
                uiState.ToggleVisibility();
            }
        }
    }
}
