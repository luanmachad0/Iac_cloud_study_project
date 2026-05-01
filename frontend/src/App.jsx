import { useMemo, useState } from "react";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080";

function formatJson(value) {
  return JSON.stringify(value, null, 2);
}

export default function App() {
  const [userId, setUserId] = useState("user-001");
  const [matchId, setMatchId] = useState("match-abc");
  const [amount, setAmount] = useState("50");
  const [prediction, setPrediction] = useState("time-casa");
  const [betId, setBetId] = useState("");
  const [isWon, setIsWon] = useState(true);
  const [output, setOutput] = useState("Pronto para testar o fluxo.");
  const [loading, setLoading] = useState(false);

  const parsedAmount = useMemo(() => Number.parseFloat(amount), [amount]);

  async function executeRequest(url, options) {
    setLoading(true);
    try {
      const response = await fetch(url, options);
      const text = await response.text();
      const data = text ? JSON.parse(text) : {};
      if (!response.ok) {
        throw new Error(formatJson(data));
      }
      setOutput(formatJson(data));
      return data;
    } catch (error) {
      setOutput(error instanceof Error ? error.message : "Erro desconhecido");
      return null;
    } finally {
      setLoading(false);
    }
  }

  async function createBet(event) {
    event.preventDefault();

    if (Number.isNaN(parsedAmount) || parsedAmount <= 0) {
      setOutput("Valor da aposta precisa ser maior que zero.");
      return;
    }

    const data = await executeRequest(`${apiBaseUrl}/api/bets`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        userId,
        matchId,
        amount: parsedAmount,
        prediction
      })
    });

    if (data?.betId) {
      setBetId(data.betId);
    }
  }

  async function settleBet() {
    if (!betId) {
      setOutput("Informe o Bet ID para liquidar.");
      return;
    }

    await executeRequest(`${apiBaseUrl}/api/bets/${betId}/settle`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ isWon })
    });
  }

  async function getBetResult() {
    if (!betId) {
      setOutput("Informe o Bet ID para consultar o resultado.");
      return;
    }

    await executeRequest(`${apiBaseUrl}/api/bets/${betId}/result`, {
      method: "GET"
    });
  }

  return (
    <main className="container">
      <h1>Sports Betting MVP</h1>
      <p>Front simples para entrevista: criar, liquidar e consultar aposta.</p>

      <section className="card">
        <h2>Criar aposta</h2>
        <form onSubmit={createBet} className="grid">
          <label>
            User ID
            <input value={userId} onChange={(e) => setUserId(e.target.value)} />
          </label>
          <label>
            Match ID
            <input value={matchId} onChange={(e) => setMatchId(e.target.value)} />
          </label>
          <label>
            Valor
            <input value={amount} onChange={(e) => setAmount(e.target.value)} />
          </label>
          <label>
            Palpite
            <input value={prediction} onChange={(e) => setPrediction(e.target.value)} />
          </label>
          <button disabled={loading} type="submit">
            {loading ? "Enviando..." : "Criar aposta"}
          </button>
        </form>
      </section>

      <section className="card">
        <h2>Operar aposta</h2>
        <label>
          Bet ID
          <input value={betId} onChange={(e) => setBetId(e.target.value)} />
        </label>
        <label className="checkbox">
          <input type="checkbox" checked={isWon} onChange={(e) => setIsWon(e.target.checked)} />
          Aposta vencedora
        </label>
        <div className="actions">
          <button disabled={loading} onClick={settleBet} type="button">
            Liquidar
          </button>
          <button disabled={loading} onClick={getBetResult} type="button">
            Consultar resultado
          </button>
        </div>
      </section>

      <section className="card">
        <h2>Resposta API</h2>
        <pre>{output}</pre>
      </section>
    </main>
  );
}
