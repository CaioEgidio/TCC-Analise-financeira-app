import { useCallback, useState } from 'react'
import type { FormEvent } from 'react'
import Dashboard from './Dashboard'
import RegisterForm from './RegisterForm'
import './App.css'

type Session = { accessToken: string; userId: string; email: string }

function App() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [session, setSession] = useState<Session | null>(null)
  const [registering, setRegistering] = useState(false)
  const [notice, setNotice] = useState('')

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) return
    setError('')
    setNotice('')
    setIsSubmitting(true)
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: email.trim(), password }),
      })
      if (response.status === 401) {
        throw new Error('Confira seu e-mail e sua senha. Se tentou várias vezes, aguarde cinco minutos e tente novamente.')
      }
      if (response.status === 400) {
        throw new Error('Confira o formato do e-mail e preencha a senha.')
      }
      if (!response.ok) {
        throw new Error('Não foi possível entrar agora. Tente novamente em instantes.')
      }
      const login: unknown = await response.json()
      if (!login || typeof login !== 'object' || !('accessToken' in login) ||
          typeof login.accessToken !== 'string' || !login.accessToken) {
        throw new Error('Não foi possível confirmar seu acesso. Tente entrar novamente.')
      }
      const accountResponse = await fetch('/api/auth/me', {
        headers: { Authorization: `Bearer ${login.accessToken}` },
      })
      if (!accountResponse.ok) {
        throw new Error('Não foi possível confirmar sua conta. Tente entrar novamente.')
      }
      const account: unknown = await accountResponse.json()
      if (!account || typeof account !== 'object' || !('id' in account) ||
          typeof account.id !== 'string' || !account.id) {
        throw new Error('A resposta da conta está incompleta. Tente novamente.')
      }
      setSession({ accessToken: login.accessToken, userId: account.id, email: email.trim() })
      setPassword('')
      setShowPassword(false)
    } catch (cause) {
      setError(cause instanceof TypeError
        ? 'Não foi possível conectar. Confira sua conexão e tente novamente.'
        : cause instanceof Error ? cause.message : 'Não foi possível entrar. Tente novamente.')
    } finally {
      setIsSubmitting(false)
    }
  }

  function handleLogout() {
    setSession(null)
    setPassword('')
    setError('')
  }

  const handleSessionExpired = useCallback(() => {
    setSession(null)
    setPassword('')
    setError('Sua sessão expirou. Entre novamente para continuar.')
  }, [])

  if (session) {
    return <Dashboard token={session.accessToken} email={session.email}
      onLogout={handleLogout} onSessionExpired={handleSessionExpired} />
  }

  return (
    <main className="login-layout" lang="pt-BR">
      <section className="brand-panel" aria-label="FinData, controle financeiro pessoal">
        <a className="brand" href="/" aria-label="FinData, início">
          <span className="brand-mark" aria-hidden="true"><i /><i /><i /></span>
          FinData
        </a>
        <div className="brand-message">
          <h1>Seu dinheiro.<br />Uma visão mais clara.</h1>
          <p>Reúna suas receitas e despesas e acompanhe o que acontece com suas finanças, mês a mês.</p>
          <div className="finance-illustration" aria-hidden="true">
            <div className="illustration-heading"><span>Seu mês em perspectiva</span><span>↗</span></div>
            <div className="illustration-row"><span className="movement-symbol">+</span><span>Receitas</span><div className="movement-track"><i /></div></div>
            <div className="illustration-row"><span className="movement-symbol">−</span><span>Despesas</span><div className="movement-track expense-track"><i /></div></div>
            <div className="illustration-footer"><span>Mais organização.</span><span>Melhores decisões.</span></div>
          </div>
        </div>
        <p className="brand-footer">Controle financeiro pessoal</p>
      </section>

      <section className="access-panel" aria-labelledby="access-title">
        <div className="access-content">
          {registering ? <RegisterForm initialEmail={email} onBack={() => setRegistering(false)}
            onRegistered={(registeredEmail) => {
              setEmail(registeredEmail)
              setPassword('')
              setShowPassword(false)
              setError('')
              setNotice('Conta criada! Entre com seu e-mail e sua senha.')
              setRegistering(false)
            }} /> : (
            <>
              <h2 id="access-title">Bom ter você por aqui.</h2>
              <p className="form-intro">Entre na sua conta para acompanhar suas finanças.</p>
              {notice && <p className="registration-success" role="status">{notice}</p>}
              <form onSubmit={handleLogin} aria-busy={isSubmitting}>
                <div className="form-field">
                  <label htmlFor="email">E-mail</label>
                  <input id="email" name="email" type="email" autoComplete="username"
                    placeholder="voce@exemplo.com" required maxLength={256}
                    value={email} onChange={(event) => setEmail(event.target.value)}
                    disabled={isSubmitting} aria-describedby={error ? 'login-error' : undefined} />
                </div>
                <div className="form-field">
                  <label htmlFor="password">Senha</label>
                  <div className="password-input">
                    <input id="password" name="password" type={showPassword ? 'text' : 'password'}
                      autoComplete="current-password" placeholder="Sua senha" required maxLength={256}
                      value={password} onChange={(event) => setPassword(event.target.value)}
                      disabled={isSubmitting} aria-describedby={error ? 'login-error' : undefined} />
                    <button className="password-toggle" type="button" aria-controls="password"
                      aria-pressed={showPassword} disabled={isSubmitting}
                      onClick={() => setShowPassword(!showPassword)}>
                      {showPassword ? 'Ocultar' : 'Mostrar'}
                    </button>
                  </div>
                </div>
                {error && <p className="form-error" id="login-error" role="alert">{error}</p>}
                <button className="primary-button" type="submit" disabled={isSubmitting}>
                  {isSubmitting ? 'Entrando…' : 'Entrar na minha conta'}
                </button>
                <p className="submit-status" role="status">{isSubmitting ? 'Confirmando seu acesso…' : ''}</p>
              </form>
              <p className="login-help">Ainda não tem uma conta? <button className="access-link" type="button" disabled={isSubmitting}
                onClick={() => { setPassword(''); setShowPassword(false); setError(''); setNotice(''); setRegistering(true) }}>Criar conta</button></p>
            </>
          )}
        </div>
        <footer className="access-footer">FinData · Organize hoje. Planeje o amanhã.</footer>
      </section>
    </main>
  )
}

export default App

