import { FormEvent, ReactNode, useEffect, useRef, useState } from "react";
import {
  BrowserRouter,
  Link,
  Navigate,
  Route,
  Routes,
  useNavigate,
  useParams,
} from "react-router-dom";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { api } from "./api";
import "./styles.css";
import "./modern.css";

type User = { id: string; displayName: string | null; email: string | null };
type League = {
  id: string;
  name: string;
  season: number;
  source: string;
  previousLeagueId: string | null;
  totalRosters: number;
};
type Team = {
  id: string;
  leagueId: string;
  ownerId: string | null;
  name: string;
  wins: number;
  losses: number;
  ties: number;
  pointsFor: number;
  pointsAgainst: number;
  managerName?: string;
};
type Player = {
  id: string;
  fullName: string;
  position: string | null;
  nflTeam: string | null;
  status: string | null;
  injuryStatus: string | null;
};
type ChatModel = { provider: string; model: string };
const Table = ({ children }: { children: ReactNode }) => (
  <div className="table-wrap">
    <table>{children}</table>
  </div>
);

function App() {
  const [user, setUser] = useState<User | null | undefined>(undefined);
  const [dark, setDark] = useState(false);
  useEffect(() => {
    api<User>("/auth/me")
      .then((result) => setUser(result))
      .catch(() => setUser(null));
  }, []);
  useEffect(() => {
    if (user)
      api<{ isDarkMode: boolean }>("/me/preferences").then((p) =>
        setDark(p.isDarkMode),
      );
  }, [user]);
  useEffect(() => {
    document.documentElement.dataset.theme = dark ? "dark" : "light";
  }, [dark]);
  if (user === undefined)
    return <main className="loading">Loading FanFoot...</main>;
  const logout = async () => {
    await api("/auth/logout", { method: "POST" });
    setUser(null);
  };
  const toggleTheme = async () => {
    const next = !dark;
    setDark(next);
    await api("/me/preferences", {
      method: "PUT",
      body: JSON.stringify({ isDarkMode: next }),
    });
  };
  return (
    <Routes>
      <Route
        path="/"
        element={
          user ? (
            <Navigate to={`/user/${user.id}`} />
          ) : (
            <SignIn onAuth={setUser} />
          )
        }
      />
      <Route
        path="*"
        element={
          !user ? (
            <Navigate to="/" />
          ) : (
            <Shell
              user={user}
              dark={dark}
              onTheme={toggleTheme}
              onLogout={logout}
            >
              <Routes>
                <Route
                  path="/user/:userId"
                  element={<UserPage user={user} />}
                />
                <Route path="/league/:leagueId" element={<LeaguePage />} />
                <Route
                  path="/league/:leagueId/team/:teamId"
                  element={<RosterPage />}
                />
                <Route path="/league/:leagueId/draft" element={<DraftPage />} />
                <Route path="/chat" element={<ChatPage />} />
                <Route
                  path="*"
                  element={<Navigate to={`/user/${user.id}`} />}
                />
              </Routes>
            </Shell>
          )
        }
      />
    </Routes>
  );
}

function SignIn({ onAuth }: { onAuth: (user: User) => void }) {
  const [register, setRegister] = useState(false);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setBusy(true);
    setError("");
    const form = new FormData(event.currentTarget);
    try {
      const result = await api<{ user: User }>(
        register ? "/auth/register" : "/auth/login",
        {
          method: "POST",
          body: JSON.stringify(
            register
              ? {
                  email: form.get("email"),
                  password: form.get("password"),
                  sleeperUsername: form.get("sleeper"),
                }
              : { email: form.get("email"), password: form.get("password") },
          ),
        },
      );
      onAuth(result.user);
    } catch {
      setError("Unable to sign in.");
    } finally {
      setBusy(false);
    }
  };
  return (
    <main className="auth">
      <form className="card" onSubmit={submit}>
        <h1>FanFoot</h1>
        <p>Fantasy football, centered on your teams.</p>
        {error && <div className="alert">{error}</div>}
        <label>
          Email
          <input required name="email" type="email" />
        </label>
        <label>
          Password
          <input required name="password" type="password" minLength={8} />
        </label>
        {register && (
          <label>
            Sleeper username
            <input required name="sleeper" />
          </label>
        )}
        <button disabled={busy}>
          {busy && register
            ? "Importing leagues..."
            : register
              ? "Create account"
              : "Sign in"}
        </button>
        <button
          className="link"
          type="button"
          onClick={() => setRegister(!register)}
        >
          {register
            ? "Already have an account? Sign in"
            : "Need an account? Register"}
        </button>
      </form>
    </main>
  );
}

function Shell({
  user,
  dark,
  onTheme,
  onLogout,
  children,
}: {
  user: User;
  dark: boolean;
  onTheme: () => void;
  onLogout: () => void;
  children: ReactNode;
}) {
  return (
    <>
      <header>
        <Link to={`/user/${user.id}`} className="brand">
          FanFoot
        </Link>
        <nav>
          <Link to={`/user/${user.id}`}>Teams</Link>
          <Link to="/chat">AI Chat</Link>
          <button className="icon" onClick={onTheme}>
            {dark ? "Light" : "Dark"}
          </button>
          <button className="icon" onClick={onLogout}>
            Sign out
          </button>
        </nav>
      </header>
      <main className="page">{children}</main>
    </>
  );
}
function useLoad<T>(path: string) {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState("");
  useEffect(() => {
    setData(null);
    api<T>(path)
      .then((result) => setData(result))
      .catch(() => setError("Request failed."));
  }, [path]);
  return { data, error };
}
function UserPage({ user }: { user: User }) {
  const { userId } = useParams();
  if (userId !== user.id) return <Navigate to={`/user/${user.id}`} />;
  const { data, error } = useLoad<{
    user: User;
    teamsBySeason: {
      season: number;
      leagues: { league: League; teams: Team[] }[];
    }[];
  }>("/me");
  if (error) return <Error text={error} />;
  if (!data) return <Loading />;
  return (
    <>
      <h1>{data.user.displayName || "My Teams"}</h1>
      {data.teamsBySeason.map((s) => (
        <section key={s.season}>
          <h2>{s.season} Season</h2>
          {s.leagues.map((g) => (
            <article className="card" key={g.league.id}>
              <h3>
                <Link to={`/league/${g.league.id}`}>{g.league.name}</Link>
              </h3>
              <TeamTable teams={g.teams} />
            </article>
          ))}
        </section>
      ))}
    </>
  );
}
function TeamTable({
  teams,
  roster = true,
}: {
  teams: Team[];
  roster?: boolean;
}) {
  return (
    <Table>
      <thead>
        <tr>
          <th>Team</th>
          <th>Manager</th>
          <th>W-L-T</th>
          <th>PF</th>
          {roster && <th />}
        </tr>
      </thead>
      <tbody>
        {teams.map((t) => (
          <tr key={t.id}>
            <td>{t.name}</td>
            <td>{t.managerName || "-"}</td>
            <td>
              {t.wins}-{t.losses}-{t.ties}
            </td>
            <td>{t.pointsFor.toFixed(1)}</td>
            {roster && (
              <td>
                <Link
                  className="button small"
                  to={`/league/${t.leagueId}/team/${t.id}`}
                >
                  Roster
                </Link>
              </td>
            )}
          </tr>
        ))}
      </tbody>
    </Table>
  );
}
function LeaguePage() {
  const { leagueId = "" } = useParams();
  const { data, error } = useLoad<{
    league: League;
    teams: Team[];
    previousLeague: League | null;
  }>(`/leagues/${leagueId}`);
  const [importing, setImporting] = useState(false);
  if (error) return <Error text={error} />;
  if (!data) return <Loading />;
  const importPrevious = async () => {
    setImporting(true);
    await api(`/leagues/${leagueId}/previous-season/import`, {
      method: "POST",
    });
    window.location.reload();
  };
  return (
    <>
      <div className="title">
        <div>
          <h1>{data.league.name}</h1>
          <p>
            Season {data.league.season} · {data.league.totalRosters} teams
          </p>
        </div>
        <div>
          {data.previousLeague ? (
            <Link
              className="button secondary"
              to={`/league/${data.previousLeague.id}`}
            >
              Previous season
            </Link>
          ) : (
            data.league.previousLeagueId && (
              <button onClick={importPrevious} disabled={importing}>
                {importing ? "Importing..." : "Import previous season"}
              </button>
            )
          )}{" "}
          <Link className="button" to={`/league/${leagueId}/draft`}>
            Draft
          </Link>
        </div>
      </div>
      <h2>Standings</h2>
      <TeamTable teams={data.teams} />
    </>
  );
}
function RosterPage() {
  const { leagueId = "", teamId = "" } = useParams();
  const { data, error } = useLoad<{
    league: League;
    team: Team;
    starters: Player[];
    bench: Player[];
    reserve: Player[];
    taxi: Player[];
  }>(`/leagues/${leagueId}/teams/${teamId}`);
  if (error) return <Error text={error} />;
  if (!data) return <Loading />;
  const section = (name: string, players: Player[]) =>
    players.length ? (
      <section>
        <h2>{name}</h2>
        <Table>
          <thead>
            <tr>
              <th>Player</th>
              <th>Pos</th>
              <th>NFL Team</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {players.map((p) => (
              <tr key={p.id}>
                <td>{p.fullName}</td>
                <td>{p.position}</td>
                <td>{p.nflTeam}</td>
                <td>{p.injuryStatus || p.status}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      </section>
    ) : null;
  return (
    <>
      <Link to={`/league/${leagueId}`}>Back to {data.league.name}</Link>
      <h1>{data.team.name}</h1>
      <p>
        {data.team.managerName} · {data.team.wins}-{data.team.losses}-
        {data.team.ties}
      </p>
      {section("Starters", data.starters)}
      {section("Bench", data.bench)}
      {section("Reserve / IR", data.reserve)}
      {section("Taxi squad", data.taxi)}
    </>
  );
}
function DraftPage() {
  const { leagueId = "" } = useParams();
  const { data, error } = useLoad<{
    league: League;
    status: string;
    type: string;
    totalPicks: number;
    picks: any[];
    plannedOrder: any[];
  }>(`/leagues/${leagueId}/draft`);
  if (error) return <Error text={error} />;
  if (!data) return <Loading />;
  const rows = data.status === "complete" ? data.picks : data.plannedOrder;
  const importPicks = async () => {
    await api(`/leagues/${leagueId}/draft/import`, { method: "POST" });
    window.location.reload();
  };
  return (
    <>
      <div className="title">
        <div>
          <h1>{data.league.name} Draft</h1>
          <p>
            {data.status === "complete" ? "Complete" : "Pre-draft"} ·{" "}
            {data.type} · {data.totalPicks} picks
          </p>
        </div>
        {data.status === "complete" && (
          <button onClick={importPicks}>Refresh draft picks</button>
        )}
      </div>
      <Table>
        <thead>
          <tr>
            <th>Round</th>
            <th>Pick</th>
            <th>Team</th>
            <th>Manager</th>
            <th>Player</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((p: any) => (
            <tr key={`${p.round}-${p.pickNumber}`}>
              <td>{p.round}</td>
              <td>{p.pickNumber}</td>
              <td>
                {p.teamName}
                {p.originalTeamName && ` (from ${p.originalTeamName})`}
              </td>
              <td>{p.managerName}</td>
              <td>{p.playerName}</td>
            </tr>
          ))}
        </tbody>
      </Table>
    </>
  );
}
function ChatPage() {
  const { data: me } = useLoad<{ currentLeagues: League[] }>("/me");
  const { data: models, error: modelsError } =
    useLoad<ChatModel[]>("/chat/models");
  const { data: sessions } =
    useLoad<{ id: string; title: string; leagueId: string | null }[]>(
      "/chat/sessions",
    );
  const [messages, setMessages] = useState<{ role: string; content: string }[]>(
    [],
  );
  const [leagueId, setLeagueId] = useState("");
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [provider, setProvider] = useState("");
  const [model, setModel] = useState("");
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const bottom = useRef<HTMLDivElement>(null);
  useEffect(() => bottom.current?.scrollIntoView(), [messages, busy]);
  useEffect(() => {
    if (!models?.length) return;
    const availableModels = models.filter((item) => item.provider === provider);
    if (!provider || !availableModels.length) {
      setProvider(models[0].provider);
      setModel(models[0].model);
    } else if (!availableModels.some((item) => item.model === model)) {
      setModel(availableModels[0].model);
    }
  }, [models, provider, model]);
  const load = async (id: string) => {
    const detail = await api<{
      session: { id: string; leagueId: string | null };
      messages: { role: string; content: string }[];
    }>(`/chat/sessions/${id}`);
    setSessionId(id);
    setLeagueId(detail.session.leagueId || "");
    setMessages(detail.messages);
  };
  const send = async (e: FormEvent) => {
    e.preventDefault();
    if (!input.trim() || busy || !provider || !model) return;
    const text = input.trim();
    setInput("");
    setError("");
    setMessages((m) => [...m, { role: "user", content: text }]);
    setBusy(true);
    try {
      const result = await api<{
        session: {
          session: { id: string; leagueId: string | null };
          messages: { role: string; content: string }[];
        };
      }>("/chat/messages", {
        method: "POST",
        body: JSON.stringify({
          sessionId,
          leagueId: leagueId || null,
          provider,
          model,
          message: text,
        }),
      });
      setSessionId(result.session.session.id);
      setMessages(result.session.messages);
    } catch (reason) {
      setError(String(reason || "Unable to get a response."));
    } finally {
      setBusy(false);
    }
  };
  return (
    <div className="chat">
      <div className="chat-toolbar">
        <h1>AI Assistant</h1>
        <label className="chat-select">
          <span>League</span>
          <select
            value={leagueId}
            onChange={(e) => {
              setLeagueId(e.target.value);
              setSessionId(null);
              setMessages([]);
            }}
          >
            <option value="">All my teams</option>
            {me?.currentLeagues.map((l) => (
              <option key={l.id} value={l.id}>
                {l.name}
              </option>
            ))}
          </select>
        </label>
        <label className="chat-select">
          <span>Provider</span>
          <select
            value={provider}
            disabled={!models?.length}
            onChange={(e) => {
              const nextProvider = e.target.value;
              setProvider(nextProvider);
              setModel(
                models?.find((item) => item.provider === nextProvider)?.model ??
                  "",
              );
            }}
          >
            {Array.from(
              new Set(models?.map((item) => item.provider) ?? []),
            ).map((item) => (
              <option key={item} value={item}>
                {item === "groq"
                  ? "Groq"
                  : item === "deepseek"
                    ? "DeepSeek"
                    : item === "ollama"
                      ? "Ollama"
                      : item}
              </option>
            ))}
          </select>
        </label>
        <label className="chat-select">
          <span>Model</span>
          <select
            value={model}
            disabled={!models?.length}
            onChange={(e) => setModel(e.target.value)}
          >
            {models
              ?.filter((item) => item.provider === provider)
              .map((item) => (
                <option key={item.model} value={item.model}>
                  {item.model}
                </option>
              ))}
          </select>
        </label>
        <label className="chat-select">
          <span>History</span>
          <select
            onChange={(e) => e.target.value && load(e.target.value)}
            value={sessionId ?? ""}
          >
            <option value="">Select chat</option>
            {sessions?.map((s) => (
              <option key={s.id} value={s.id}>
                {s.title}
              </option>
            ))}
          </select>
        </label>
        <button
          onClick={() => {
            setSessionId(null);
            setMessages([]);
          }}
        >
          New chat
        </button>
      </div>
      <div className="messages">
        {modelsError && (
          <div className="alert" role="alert">
            Unable to load available AI models.
          </div>
        )}
        {error && (
          <div className="alert" role="alert">
            {error}
          </div>
        )}
        {messages.length === 0 && (
          <p>Ask about your teams, players, or matchups.</p>
        )}
        {messages.map((m, i) => (
          <div
            key={i}
            className={`bubble ${m.role}`}
            aria-live={m.role === "assistant" ? "polite" : undefined}
          >
            {m.role === "assistant" ? (
              <ReactMarkdown remarkPlugins={[remarkGfm]}>
                {m.content}
              </ReactMarkdown>
            ) : (
              m.content
            )}
          </div>
        ))}
        {busy && (
          <div className="bubble assistant" role="status">
            Thinking...
          </div>
        )}
        <div ref={bottom} />
      </div>
      <form className="composer" onSubmit={send}>
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Ask anything..."
          disabled={busy}
        />
        <button disabled={busy || !provider || !model}>Send</button>
      </form>
    </div>
  );
}
function Loading() {
  return <div className="loading">Loading...</div>;
}
function Error({ text }: { text: string }) {
  return <div className="alert">{text}</div>;
}
window.document.title = "FanFoot";
window.document.getElementById("root") && __importReact();
function __importReact() {
  import("react-dom/client").then(({ createRoot }) =>
    createRoot(document.getElementById("root")!).render(
      <BrowserRouter>
        <App />
      </BrowserRouter>,
    ),
  );
}
