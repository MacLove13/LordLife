# Sistema de Diálogos - LordLife

Este guia explica como funciona o sistema de diálogos do LordLife e como adicionar novos textos de diálogo com variações de respostas e ganho ou perda de relacionamento.

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Estrutura do Sistema](#estrutura-do-sistema)
3. [Tipos de Diálogo](#tipos-de-diálogo)
4. [Como Adicionar Novos Diálogos](#como-adicionar-novos-diálogos)
5. [Variações de Respostas](#variações-de-respostas)
6. [Sistema de Relacionamento](#sistema-de-relacionamento)
7. [Sistema de Cooldown](#sistema-de-cooldown)
8. [Exemplos Práticos](#exemplos-práticos)
9. [Boas Práticas](#boas-práticas)

---

## Visão Geral

O sistema de diálogos do LordLife permite criar conversas dinâmicas com NPCs que:
- Possuem múltiplas variações de respostas
- Afetam o relacionamento com o NPC
- Têm cooldowns personalizados
- Desbloqueiam baseado no nível de relacionamento
- Respondem a eventos do jogo (guerras, mortes)

---

## Estrutura do Sistema

O sistema é composto por três arquivos principais:

### 1. **DialogueData.cs**
Define as estruturas de dados básicas:
- `DialogueEntry`: Representa uma opção de diálogo
- `DialogueResponse`: Representa uma possível resposta do NPC
- `DialogueType`: Enum com os tipos de diálogo
- `DialogueCooldownEntry`: Gerencia o estado de cooldown

### 2. **DialogueDefinitions.cs**
Contém **todos os diálogos do mod**. É aqui que você adiciona novos diálogos!

### 3. **DialogueCampaignBehavior.cs**
Gerencia o sistema de diálogos no jogo (não precisa ser modificado).

---

## Tipos de Diálogo

### 1. **Basic** (Básico)
- Diálogos disponíveis para todos os NPCs
- Cooldown simples baseado em dias
- Sem requisito de relacionamento mínimo

**Uso:** Conversas gerais, assuntos cotidianos

### 2. **Relationship** (Relacionamento)
- Desbloqueados ao atingir um nível mínimo de relacionamento
- Cooldown baseado em dias
- Permitem conversas mais profundas

**Uso:** Diálogos pessoais, amizade, confiança

### 3. **War** (Guerra)
- Disponíveis apenas quando ambos estão no mesmo reino em guerra
- Resetam quando uma nova guerra começa
- Usados apenas uma vez por guerra

**Uso:** Conversas sobre táticas, inimigos, estratégia militar

### 4. **DeathCondolence** (Condolências)
- Aparecem quando um parente do NPC morre
- Usados apenas uma vez por morte
- Texto gerado dinamicamente com o nome do falecido

**Uso:** Prestar condolências por perda de familiares

---

## Como Adicionar Novos Diálogos

### Passo 1: Abra o arquivo DialogueDefinitions.cs

Localize a lista `AllDialogues` dentro da classe `DialogueDefinitions`.

### Passo 2: Adicione um novo DialogueEntry

```csharp
new DialogueEntry(
    id: "seu_id_unico",                    // ID único para identificar o diálogo
    playerText: "Texto que o jogador verá no menu",
    responses: new List<DialogueResponse>   // Lista de possíveis respostas
    {
        new DialogueResponse("Primeira variação de resposta", 1),
        new DialogueResponse("Segunda variação de resposta", 0),
        new DialogueResponse("Terceira variação de resposta", 2)
    },
    type: DialogueType.Basic,               // Tipo do diálogo
    minRelationship: 0,                     // Relacionamento mínimo necessário
    cooldownDays: 3,                        // Dias até poder usar novamente
    priority: 100)                          // Prioridade no menu (maior = mais alto)
```

### Passo 3: Salve o arquivo e recompile o mod

```bash
dotnet build -c Release
```

---

## Variações de Respostas

O sistema tecnicamente suporta qualquer quantidade de variações de respostas, mas **todos os diálogos existentes usam exatamente 3 respostas**. **É fortemente recomendado seguir este padrão estabelecido** de usar 3 respostas para manter a consistência em todo o sistema.

**Por que 3 respostas é o padrão estabelecido?**
- Oferece variedade suficiente sem sobrecarregar o sistema
- Mantém consistência com todos os diálogos existentes do mod
- Permite criar respostas com diferentes tons e impactos no relacionamento
- Facilita manutenção e revisão de diálogos

### Como Funciona

Quando o jogador seleciona um diálogo, o sistema escolhe **aleatoriamente** uma das respostas disponíveis. Isso torna as conversas mais dinâmicas e imprevisíveis.

### Estrutura de uma Resposta

```csharp
new DialogueResponse(
    text: "Texto que o NPC dirá",           // Obrigatório
    relationshipChange: 2                    // Opcional: mudança no relacionamento
)
```

### Valores de RelationshipChange

- **Positivo (+1, +2, +3, etc.)**: Aumenta o relacionamento
- **Negativo (-1, -2, -3, etc.)**: Diminui o relacionamento
- **Zero (0)**: Sem mudança no relacionamento

### Exemplo com Variações

```csharp
responses: new List<DialogueResponse>
{
    // Resposta muito positiva
    new DialogueResponse("Que bom te ver! Sempre é um prazer conversar.", 3),
    
    // Resposta neutra
    new DialogueResponse("Tudo bem. E com você?", 0),
    
    // Resposta levemente positiva
    new DialogueResponse("Estou bem, obrigado por perguntar.", 1)
}
```

---

## Sistema de Relacionamento

### Níveis de Relacionamento

O relacionamento no Bannerlord vai de **-100 a +100**. Aqui estão alguns pontos de referência úteis:

| Nível | Descrição Sugerida |
|-------|-----------|
| 0-19  | Conhecido |
| 20-49 | Amigável |
| 50-79 | Amigo |
| 80+   | Aliado Próximo |

**Nota:** Estas são categorias sugeridas para organizar seus diálogos. O jogo não possui divisões rígidas desses níveis.

### Requisito Mínimo (minRelationship)

Define o relacionamento mínimo necessário para o diálogo aparecer:

```csharp
minRelationship: 0    // Qualquer um pode ver
minRelationship: 20   // Apenas amigáveis ou superiores
minRelationship: 50   // Apenas amigos ou superiores
minRelationship: 80   // Apenas aliados próximos
```

### Balanceamento de Ganhos

**Limites máximos recomendados por tipo de diálogo:**
- **Diálogos Básicos**: +0 a +2 (máximo: +2)
- **Diálogos de Relacionamento 20+**: +1 a +3 (máximo: +3)
- **Diálogos de Relacionamento 50+**: +2 a +4 (máximo: +4)
- **Diálogos de Relacionamento 80+**: +3 a +5 (máximo: +5)
- **Diálogos de Guerra**: +1 a +2 (máximo: +2)
- **Condolências**: +3 a +5 (máximo: +5)

**Perdas de relacionamento:**
- Use com muito cuidado e raramente
- Máximo recomendado: -3
- Sempre ofereça contexto claro de por que haveria uma perda

---

## Sistema de Cooldown

### Cooldown para Diálogos Basic e Relationship

Define quantos dias devem passar antes do diálogo poder ser usado novamente com o mesmo NPC:

```csharp
cooldownDays: 3   // 3 dias
cooldownDays: 7   // 1 semana
cooldownDays: 14  // 2 semanas
```

**Recomendações:**
- Diálogos simples: 3-5 dias
- Diálogos pessoais: 5-10 dias
- Diálogos profundos: 10-20 dias

### Cooldown para Diálogos War

Não usa dias. O diálogo reseta automaticamente quando:
- Uma nova guerra começa
- A guerra atual termina e uma nova inicia

```csharp
type: DialogueType.War,
cooldownDays: 0  // Ignorado para tipo War
```

### Cooldown para Diálogos DeathCondolence

Não usa dias. O diálogo pode ser usado:
- Uma vez para cada parente falecido
- Reseta quando outro parente do NPC morre

```csharp
type: DialogueType.DeathCondolence,
cooldownDays: 0  // Ignorado para tipo DeathCondolence
```

---

## Exemplos Práticos

### Exemplo 1: Diálogo Básico Simples

```csharp
new DialogueEntry(
    id: "basic_greetings",
    playerText: "Como você está hoje?",
    responses: new List<DialogueResponse>
    {
        new DialogueResponse("Estou bem, obrigado!", 1),
        new DialogueResponse("Poderia estar melhor, mas seguimos.", 0),
        new DialogueResponse("Excelente! E você?", 2)
    },
    type: DialogueType.Basic,
    minRelationship: 0,
    cooldownDays: 3,
    priority: 100)
```

**Quando aparece:** Sempre disponível para todos  
**Cooldown:** 3 dias  
**Efeito:** Pode aumentar relacionamento em +1 ou +2

---

### Exemplo 2: Diálogo de Relacionamento (Amigos)

```csharp
new DialogueEntry(
    id: "rel50_personal_struggles",
    playerText: "Você tem enfrentado alguma dificuldade ultimamente?",
    responses: new List<DialogueResponse>
    {
        new DialogueResponse("Obrigado por se importar. Tenho sim, mas vou superar.", 3),
        new DialogueResponse("Nada que eu não possa lidar. Mas agradeço por perguntar.", 2),
        new DialogueResponse("Sim, mas falar sobre isso com você me ajuda.", 4)
    },
    type: DialogueType.Relationship,
    minRelationship: 50,
    cooldownDays: 7,
    priority: 85)
```

**Quando aparece:** Relacionamento >= 50  
**Cooldown:** 7 dias  
**Efeito:** Aumenta relacionamento em +2 a +4

---

### Exemplo 3: Diálogo de Guerra

```csharp
new DialogueEntry(
    id: "war_strategy_discussion",
    playerText: "Precisamos de uma boa estratégia para vencer esta guerra.",
    responses: new List<DialogueResponse>
    {
        new DialogueResponse("Concordo. Já estou pensando em algumas possibilidades.", 2),
        new DialogueResponse("Estratégia é tudo. Vamos discutir isso em detalhes.", 2),
        new DialogueResponse("Com sua experiência e a minha, venceremos!", 3)
    },
    type: DialogueType.War,
    minRelationship: 0,
    cooldownDays: 0,
    priority: 110)
```

**Quando aparece:** Apenas durante guerra, no mesmo reino  
**Cooldown:** Até a próxima guerra  
**Efeito:** Aumenta relacionamento em +2 a +3

---

### Exemplo 4: Diálogo de Condolências (Especial)

```csharp
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
```

**Quando aparece:** Quando um parente do NPC morre  
**Cooldown:** Até que outro parente morra  
**Efeito:** Aumenta relacionamento em +3 a +4  
**Nota:** `{RELATIVE_NAME}` é substituído automaticamente pelo nome do falecido

---

### Exemplo 5: Diálogo com Perda de Relacionamento

```csharp
new DialogueEntry(
    id: "basic_insult_politics",
    playerText: "Suas decisões políticas têm sido questionáveis.",
    responses: new List<DialogueResponse>
    {
        new DialogueResponse("Cuide da sua própria vida!", -3),
        new DialogueResponse("Não me importo com sua opinião.", -2),
        new DialogueResponse("Cada um tem direito à sua visão, mesmo que errada.", -1)
    },
    type: DialogueType.Basic,
    minRelationship: 0,
    cooldownDays: 10,
    priority: 50)
```

**Quando aparece:** Sempre disponível  
**Cooldown:** 10 dias  
**Efeito:** Diminui relacionamento em -1 a -3  
**Nota:** Use valores negativos com cuidado!

---

## Boas Práticas

### 1. **IDs Únicos**
Sempre use IDs descritivos e únicos:
```csharp
✅ id: "basic_ask_about_day"
✅ id: "rel50_share_concerns"
✅ id: "war_tactics"
❌ id: "dialogue1"
❌ id: "test"
```

### 2. **Convenção de Nomenclatura**
Use prefixos para organizar:
- `basic_` - Diálogos básicos
- `rel20_` - Diálogos para relacionamento 20+
- `rel50_` - Diálogos para relacionamento 50+
- `rel80_` - Diálogos para relacionamento 80+
- `war_` - Diálogos de guerra
- `death_` - Diálogos de condolência

### 3. **Sempre Tenha 3 Variações**
Isso torna as conversas mais interessantes:
```csharp
✅ 3 respostas diferentes
⚠️ 2 respostas (funciona, mas menos interessante)
❌ 1 resposta (muito repetitivo)
```

### 4. **Varie os Valores de Relacionamento**
Nem todas as respostas precisam dar o mesmo ganho:
```csharp
responses: new List<DialogueResponse>
{
    new DialogueResponse("Resposta muito positiva", 3),
    new DialogueResponse("Resposta neutra", 0),
    new DialogueResponse("Resposta levemente positiva", 1)
}
```

### 5. **Prioridades Lógicas**
- **120+**: Diálogos especiais (condolências)
- **110-119**: Diálogos de guerra
- **100-109**: Diálogos básicos
- **90-99**: Diálogos de relacionamento 20+
- **80-89**: Diálogos de relacionamento 50+
- **70-79**: Diálogos de relacionamento 80+

### 6. **Cooldowns Apropriados**
- Diálogos casuais: 3-5 dias
- Diálogos pessoais: 5-10 dias
- Diálogos profundos: 10-20 dias
- Guerra/Morte: 0 (gerenciado automaticamente)

### 7. **Contexto Cultural e Histórico**
Lembre-se que Bannerlord é ambientado em uma era medieval:
```csharp
✅ "Que os deuses te abençoem!"
✅ "Pela honra do reino!"
❌ "Manda um WhatsApp depois!"
❌ "Vou postar isso no Instagram!"
```

### 8. **Teste Seus Diálogos**
Após adicionar novos diálogos:
1. Compile o mod
2. Carregue um save
3. Converse com diferentes NPCs
4. Verifique se o texto aparece corretamente
5. Confirme que o relacionamento muda como esperado

### 9. **Balanceamento**
- **Não exceda os valores máximos recomendados!** Por exemplo, um diálogo básico dando +10 de relacionamento quebraria o balanceamento do jogo, permitindo progressão muito rápida (em 5 conversas você iria de 0 a 50 de relacionamento). Valores altos também são irrealistas para conversas simples. Siga os máximos listados na seção "Balanceamento de Ganhos".
- Evite cooldowns muito curtos (< 2 dias) - o jogador poderia explorar isso para ganhar relacionamento muito rapidamente
- Evite cooldowns muito longos (> 30 dias) para diálogos básicos - isso tornaria o sistema frustrante de usar

### 10. **Documentação Inline**
Adicione comentários antes de grupos de diálogos:
```csharp
// =====================================================
// DIÁLOGOS SOBRE COMÉRCIO
// =====================================================
new DialogueEntry(
    id: "basic_trade_prices",
    ...
)
```

---

## Troubleshooting

### Meu diálogo não aparece no jogo

**Possíveis causas:**
1. ✅ Verifique se o relacionamento mínimo está correto
2. ✅ Confirme que o cooldown expirou
3. ✅ Para diálogos de guerra, confirme que ambos estão no mesmo reino em guerra
4. ✅ Certifique-se de que o mod foi recompilado após a alteração
5. ✅ Verifique se o ID é único (sem duplicatas)

### O relacionamento não está mudando

**Possíveis causas:**
1. ✅ Verifique o valor de `relationshipChange` na resposta
2. ✅ Confirme que o jogo está mostrando a mensagem de mudança de relacionamento
3. ✅ Teste com um NPC diferente

### O texto está aparecendo errado

**Possíveis causas:**
1. ✅ Certifique-se de usar aspas duplas corretamente
2. ✅ Escape caracteres especiais se necessário: `\"` 
3. ✅ Para condolências, use `{RELATIVE_NAME}` no lugar correto

---

## Contribuindo

Ao adicionar novos diálogos para o mod:

1. **Siga as convenções** descritas neste guia
2. **Teste extensivamente** seus diálogos
3. **Documente mudanças significativas** nos comentários do código
4. **Mantenha a qualidade** do texto em português
5. **Balanceie os valores** de relacionamento apropriadamente

---

## Estrutura do Código

Para referência rápida:

```csharp
new DialogueEntry(
    id: "identificador_unico",              // String: ID único
    playerText: "Texto do jogador",         // String: O que o jogador diz
    responses: new List<DialogueResponse>   // Lista: Respostas possíveis
    {
        new DialogueResponse("Texto NPC", relacionamento_mudança),
        // ... até 3 respostas
    },
    type: DialogueType.Tipo,                // Enum: Basic, Relationship, War, DeathCondolence
    minRelationship: 0,                     // Int: 0, 20, 50, 80, etc.
    cooldownDays: 3,                        // Int: dias de cooldown (0 para War/Death)
    priority: 100)                          // Int: ordem no menu (maior = mais alto)
```

---

## Recursos Adicionais

- **DialogueData.cs**: Estruturas de dados e enums
- **DialogueDefinitions.cs**: Todos os diálogos do mod (EDITE AQUI!)
- **DialogueCampaignBehavior.cs**: Lógica do sistema (não edite)

---

## Changelog do Sistema

### Versão Atual
- ✅ Sistema de diálogos básicos
- ✅ Sistema de relacionamento por níveis
- ✅ Diálogos de guerra
- ✅ Diálogos de condolência
- ✅ Cooldowns personalizados
- ✅ Variações de respostas
- ✅ Submenu de conversação

---

**Feito com ❤️ para a comunidade Bannerlord**

Se tiver dúvidas ou sugestões, abra uma issue no GitHub!
