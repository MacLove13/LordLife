using System.Collections.Generic;

namespace Bannerlord.LordLife.Dialogues
{
    /// <summary>
    /// Contains all dialogue definitions for the mod.
    /// Add new dialogues here by creating new DialogueEntry instances.
    /// </summary>
    public static class DialogueDefinitions
    {
        /// <summary>
        /// All available dialogue entries.
        /// To add a new dialogue:
        /// 1. Create a new DialogueEntry with a unique ID
        /// 2. Add responses (up to 3 variations)
        /// 3. Set the type, relationship requirement, cooldown, and priority
        /// </summary>
        public static List<DialogueEntry> AllDialogues { get; } = new List<DialogueEntry>
        {
            // =====================================================
            // BASIC DIALOGUES (Available to everyone)
            // =====================================================
            new DialogueEntry(
                id: "basic_ask_about_day",
                playerText: "Como está sendo seu dia?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Tem sido um dia tranquilo, obrigado por perguntar.", 1),
                    new DialogueResponse("Os deveres me mantêm ocupado, mas não posso reclamar.", 0),
                    new DialogueResponse("Dias difíceis, mas seguimos em frente. E o seu?", 1)
                },
                type: DialogueType.Basic,
                minRelationship: 0,
                cooldownDays: 3,
                priority: 100),

            new DialogueEntry(
                id: "basic_recent_events",
                playerText: "O que você acha dos acontecimentos recentes?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Os tempos são turbulentos, mas temos que nos adaptar.", 0),
                    new DialogueResponse("Há muitas notícias circulando. É difícil saber no que acreditar.", 0),
                    new DialogueResponse("Prefiro me concentrar no que posso controlar. O resto, deixo para o destino.", 1)
                },
                type: DialogueType.Basic,
                minRelationship: 0,
                cooldownDays: 5,
                priority: 99),

            new DialogueEntry(
                id: "basic_local_news",
                playerText: "Há novidades por aqui?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Nada de especial, apenas o de sempre.", 0),
                    new DialogueResponse("Os mercadores estão reclamando dos bandidos nas estradas.", 0),
                    new DialogueResponse("Ouvi dizer que houve algumas mudanças nas terras próximas.", 0)
                },
                type: DialogueType.Basic,
                minRelationship: 0,
                cooldownDays: 4,
                priority: 98),

            new DialogueEntry(
                id: "basic_trade_routes",
                playerText: "Como estão as rotas comerciais?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Os mercadores reclamam, mas o comércio continua.", 0),
                    new DialogueResponse("Algumas rotas estão perigosas, mas outras prosperam.", 0),
                    new DialogueResponse("O ouro continua fluindo, mesmo em tempos difíceis.", 0)
                },
                type: DialogueType.Basic,
                minRelationship: 0,
                cooldownDays: 5,
                priority: 97),

            // =====================================================
            // RELATIONSHIP 20+ DIALOGUES
            // =====================================================
            new DialogueEntry(
                id: "rel20_personal_life",
                playerText: "Como vai a vida pessoal?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Agradeço o interesse. As coisas vão bem, dentro do possível.", 2),
                    new DialogueResponse("Tenho meus desafios, como todos, mas estou bem.", 1),
                    new DialogueResponse("É gentil da sua parte perguntar. Tenho muito pelo que agradecer.", 2)
                },
                type: DialogueType.Relationship,
                minRelationship: 20,
                cooldownDays: 5,
                priority: 90),

            new DialogueEntry(
                id: "rel20_family",
                playerText: "Como está sua família?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Todos estão bem, obrigado por perguntar. Sua consideração me honra.", 2),
                    new DialogueResponse("Temos nossas preocupações, mas estamos unidos.", 1),
                    new DialogueResponse("A família é o que mais importa. Estou grato por tê-los.", 2)
                },
                type: DialogueType.Relationship,
                minRelationship: 20,
                cooldownDays: 6,
                priority: 89),

            new DialogueEntry(
                id: "rel20_opinion_lords",
                playerText: "O que você acha dos outros lordes da região?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Alguns são honrados, outros... bem, você sabe como é.", 1),
                    new DialogueResponse("Cada um tem suas ambições. Cabe a nós escolher nossos aliados com sabedoria.", 1),
                    new DialogueResponse("Há aqueles em quem confio e outros que observo com cautela.", 0)
                },
                type: DialogueType.Relationship,
                minRelationship: 20,
                cooldownDays: 7,
                priority: 88),

            // =====================================================
            // RELATIONSHIP 50+ DIALOGUES
            // =====================================================
            new DialogueEntry(
                id: "rel50_trusted_friend",
                playerText: "Considero você um amigo de confiança.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("E eu a você. Em tempos difíceis, amigos verdadeiros são raros.", 3),
                    new DialogueResponse("Suas palavras me honram. Pode contar comigo quando precisar.", 3),
                    new DialogueResponse("A confiança mútua é a base de qualquer aliança duradoura. Obrigado.", 2)
                },
                type: DialogueType.Relationship,
                minRelationship: 50,
                cooldownDays: 10,
                priority: 80),

            new DialogueEntry(
                id: "rel50_share_concerns",
                playerText: "Posso compartilhar algumas preocupações com você?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Claro, estou aqui para ouvir. O que o aflige?", 2),
                    new DialogueResponse("Amigos estão aqui para isso. Fale sem receio.", 2),
                    new DialogueResponse("Confie em mim como eu confio em você. Estou ouvindo.", 3)
                },
                type: DialogueType.Relationship,
                minRelationship: 50,
                cooldownDays: 7,
                priority: 79),

            new DialogueEntry(
                id: "rel50_advice",
                playerText: "Gostaria de ouvir seu conselho sobre algo.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Darei minha opinião honesta, mesmo que não seja o que deseja ouvir.", 2),
                    new DialogueResponse("Fico honrado que valorize minha perspectiva. Pergunte.", 2),
                    new DialogueResponse("Entre amigos, a honestidade vem primeiro. O que precisa saber?", 2)
                },
                type: DialogueType.Relationship,
                minRelationship: 50,
                cooldownDays: 8,
                priority: 78),

            new DialogueEntry(
                id: "rel50_secret_plans",
                playerText: "Você tem planos para o futuro que gostaria de compartilhar?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Entre nós, tenho algumas ideias. Mas os tempos exigem cautela.", 1),
                    new DialogueResponse("Planejamentos são feitos para serem adaptados. Por enquanto, observo.", 0),
                    new DialogueResponse("Meus planos dependem de como os eventos se desenrolam. Mas tenho esperanças.", 1)
                },
                type: DialogueType.Relationship,
                minRelationship: 50,
                cooldownDays: 10,
                priority: 77),

            // =====================================================
            // RELATIONSHIP 80+ DIALOGUES
            // =====================================================
            new DialogueEntry(
                id: "rel80_deep_bond",
                playerText: "Nossa amizade significa muito para mim.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("E para mim também. Poucos conquistam minha lealdade como você.", 5),
                    new DialogueResponse("Palavras assim valem mais que ouro. Saiba que são recíprocas.", 5),
                    new DialogueResponse("Em um mundo de intrigas, uma amizade verdadeira é um tesouro.", 4)
                },
                type: DialogueType.Relationship,
                minRelationship: 80,
                cooldownDays: 14,
                priority: 70),

            new DialogueEntry(
                id: "rel80_sworn_ally",
                playerText: "Saiba que pode contar comigo em qualquer situação.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("E você comigo. Até o fim, se necessário.", 5),
                    new DialogueResponse("Juntos, somos mais fortes. Nunca esqueça disso.", 4),
                    new DialogueResponse("Sua lealdade será recompensada. Juro pela minha honra.", 5)
                },
                type: DialogueType.Relationship,
                minRelationship: 80,
                cooldownDays: 14,
                priority: 69),

            new DialogueEntry(
                id: "rel80_deepest_secrets",
                playerText: "Há algo que nunca contou a ninguém?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Para você, abrirei uma exceção. Mas isso fica entre nós.", 3),
                    new DialogueResponse("Há coisas do passado que prefiro deixar enterradas. Mas confio em você.", 2),
                    new DialogueResponse("Todos temos segredos. Talvez um dia eu compartilhe os meus.", 1)
                },
                type: DialogueType.Relationship,
                minRelationship: 80,
                cooldownDays: 20,
                priority: 68),

            new DialogueEntry(
                id: "rel80_legacy",
                playerText: "Como você gostaria de ser lembrado?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Como alguém que fez a diferença. Que protegeu os seus.", 3),
                    new DialogueResponse("Espero deixar um mundo melhor do que encontrei. É tudo o que posso desejar.", 3),
                    new DialogueResponse("A história julgará. Espero apenas ter sido justo.", 2)
                },
                type: DialogueType.Relationship,
                minRelationship: 80,
                cooldownDays: 15,
                priority: 67),

            // =====================================================
            // WAR DIALOGUES (Same kingdom at war)
            // =====================================================
            new DialogueEntry(
                id: "war_tactics",
                playerText: "Vamos discutir táticas de guerra.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Precisamos ser estratégicos. O inimigo não perdoará erros.", 1),
                    new DialogueResponse("A melhor defesa é um bom ataque. Devemos pressionar suas linhas.", 1),
                    new DialogueResponse("Conhecer o terreno é essencial. Cada vantagem conta.", 2)
                },
                type: DialogueType.War,
                minRelationship: 0,
                cooldownDays: 0,
                priority: 110),

            new DialogueEntry(
                id: "war_enemy_locations",
                playerText: "Quero comentar sobre locais inimigos que passei.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Informações assim são valiosas. O que você observou?", 2),
                    new DialogueResponse("Qualquer detalhe pode fazer diferença. Continue.", 1),
                    new DialogueResponse("Bom trabalho em reconhecer o terreno inimigo. Isso nos ajudará.", 2)
                },
                type: DialogueType.War,
                minRelationship: 0,
                cooldownDays: 0,
                priority: 109),

            new DialogueEntry(
                id: "war_enemy_troops",
                playerText: "Tenho informações sobre tropas inimigas.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Excelente! Conhecer a força do inimigo é metade da batalha.", 2),
                    new DialogueResponse("Quantos são? Que tipos de unidades? Todo detalhe importa.", 1),
                    new DialogueResponse("Com essas informações, podemos planejar melhor nossa defesa.", 2)
                },
                type: DialogueType.War,
                minRelationship: 0,
                cooldownDays: 0,
                priority: 108),

            new DialogueEntry(
                id: "war_morale",
                playerText: "Como está o moral das tropas?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Os homens estão determinados. Sabem pelo que lutam.", 1),
                    new DialogueResponse("Algumas vitórias ajudariam, mas estamos firmes.", 0),
                    new DialogueResponse("Enquanto tivermos esperança, lutaremos. E temos.", 1)
                },
                type: DialogueType.War,
                minRelationship: 0,
                cooldownDays: 0,
                priority: 107),

            new DialogueEntry(
                id: "war_supplies",
                playerText: "Nossos suprimentos estão adequados para a guerra?",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Por enquanto, sim. Mas uma guerra prolongada exigirá mais recursos.", 0),
                    new DialogueResponse("Precisamos garantir nossas linhas de abastecimento.", 0),
                    new DialogueResponse("Os armazéns estão cheios, mas devemos ser prudentes.", 1)
                },
                type: DialogueType.War,
                minRelationship: 0,
                cooldownDays: 0,
                priority: 106),

            // =====================================================
            // DEATH CONDOLENCE DIALOGUE
            // This is a special entry - the actual text will be
            // dynamically generated with the relative's name
            // =====================================================
            new DialogueEntry(
                id: "death_condolence",
                playerText: "Gostaria de prestar lembranças a {RELATIVE_NAME}.",
                responses: new List<DialogueResponse>
                {
                    new DialogueResponse("Agradeço suas palavras. {RELATIVE_NAME} faz muita falta.", 3),
                    new DialogueResponse("É reconfortante saber que outros também se lembram. Obrigado.", 3),
                    new DialogueResponse("Sua consideração em tempos difíceis significa muito para mim.", 4)
                },
                type: DialogueType.DeathCondolence,
                minRelationship: 0,
                cooldownDays: 0,
                priority: 120)
        };

        /// <summary>
        /// Gets all basic dialogues (no special requirements).
        /// </summary>
        public static IEnumerable<DialogueEntry> GetBasicDialogues()
        {
            foreach (var dialogue in AllDialogues)
            {
                if (dialogue.Type == DialogueType.Basic)
                {
                    yield return dialogue;
                }
            }
        }

        /// <summary>
        /// Gets all relationship-based dialogues for a given relationship level.
        /// </summary>
        public static IEnumerable<DialogueEntry> GetRelationshipDialogues(int relationshipLevel)
        {
            foreach (var dialogue in AllDialogues)
            {
                if (dialogue.Type == DialogueType.Relationship && relationshipLevel >= dialogue.MinRelationship)
                {
                    yield return dialogue;
                }
            }
        }

        /// <summary>
        /// Gets all war-related dialogues.
        /// </summary>
        public static IEnumerable<DialogueEntry> GetWarDialogues()
        {
            foreach (var dialogue in AllDialogues)
            {
                if (dialogue.Type == DialogueType.War)
                {
                    yield return dialogue;
                }
            }
        }

        /// <summary>
        /// Gets the death condolence dialogue.
        /// </summary>
        public static DialogueEntry? GetDeathCondolenceDialogue()
        {
            foreach (var dialogue in AllDialogues)
            {
                if (dialogue.Type == DialogueType.DeathCondolence)
                {
                    return dialogue;
                }
            }
            return null;
        }
    }
}
