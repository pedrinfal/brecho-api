import {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
  type ReactNode,
} from 'react';
import { type AuthResposta, registrarManipulador401 } from './api';

export interface DadosSessao {
  id: number;
  nome: string;
  email: string;
  papel: 'admin' | 'cliente';
  token: string;
  expiraEm: string;
}

export interface MensagemToast {
  id: number;
  tipo: 'sucesso' | 'erro' | 'info';
  texto: string;
}

interface ContextoSessaoTipo {
  sessao: DadosSessao | null;
  entrar: (dados: AuthResposta) => void;
  sair: (aviso?: string) => void;
  tema: 'claro' | 'escuro';
  alternarTema: () => void;
  toasts: MensagemToast[];
  adicionarToast: (tipo: 'sucesso' | 'erro' | 'info', texto: string) => void;
  removerToast: (id: number) => void;
}

const CHAVE_STORAGE = 'brecho_sessao_v1';
const CHAVE_TEMA = 'brecho_tema';

const ContextoSessao = createContext<ContextoSessaoTipo | undefined>(undefined);

function lerSessaoSalva(): DadosSessao | null {
  try {
    const bruto = localStorage.getItem(CHAVE_STORAGE);
    if (!bruto) return null;
    const dados = JSON.parse(bruto) as DadosSessao;
    if (!dados.token || !dados.expiraEm) return null;
    if (new Date(dados.expiraEm).getTime() <= Date.now()) {
      localStorage.removeItem(CHAVE_STORAGE);
      return null;
    }
    return dados;
  } catch {
    return null;
  }
}

function lerTemaInicial(): 'claro' | 'escuro' {
  const atr = document.documentElement.getAttribute('data-tema');
  if (atr === 'escuro' || atr === 'claro') return atr;
  return 'claro';
}

export function ProvedorSessao({ children }: { children: ReactNode }) {
  const [sessao, setSessao] = useState<DadosSessao | null>(() => lerSessaoSalva());
  const [tema, setTema] = useState<'claro' | 'escuro'>(() => lerTemaInicial());
  const [toasts, setToasts] = useState<MensagemToast[]>([]);

  const removerToast = useCallback((id: number) => {
    setToasts((atual) => atual.filter((t) => t.id !== id));
  }, []);

  const adicionarToast = useCallback(
    (tipo: 'sucesso' | 'erro' | 'info', texto: string) => {
      const id = Date.now() + Math.floor(Math.random() * 1000);
      setToasts((atual) => [...atual, { id, tipo, texto }]);
      window.setTimeout(() => {
        removerToast(id);
      }, 4500);
    },
    [removerToast]
  );

  const sair = useCallback(
    (aviso?: string) => {
      localStorage.removeItem(CHAVE_STORAGE);
      setSessao(null);
      if (aviso) {
        adicionarToast('info', aviso);
      }
    },
    [adicionarToast]
  );

  const entrar = useCallback(
    (dados: AuthResposta) => {
      const novaSessao: DadosSessao = {
        id: dados.id,
        nome: dados.nome,
        email: dados.email,
        papel: dados.papel,
        token: dados.token,
        expiraEm: dados.expiraEm,
      };
      localStorage.setItem(CHAVE_STORAGE, JSON.stringify(novaSessao));
      setSessao(novaSessao);
    },
    []
  );

  useEffect(() => {
    registrarManipulador401(() => {
      sair('Sua sessão expirou. Faça login novamente.');
    });
  }, [sair]);

  useEffect(() => {
    if (!sessao) return;
    const msRestantes = new Date(sessao.expiraEm).getTime() - Date.now();
    if (msRestantes <= 0) {
      sair('Sua sessão expirou.');
      return;
    }
    const timer = window.setTimeout(() => {
      sair('Sua sessão expirou.');
    }, msRestantes);
    return () => window.clearTimeout(timer);
  }, [sessao, sair]);

  const alternarTema = useCallback(() => {
    setTema((atual) => {
      const proximo = atual === 'claro' ? 'escuro' : 'claro';
      document.documentElement.setAttribute('data-tema', proximo);
      try {
        localStorage.setItem(CHAVE_TEMA, proximo);
      } catch {
        // ignora erro de storage
      }
      return proximo;
    });
  }, []);

  return (
    <ContextoSessao.Provider
      value={{
        sessao,
        entrar,
        sair,
        tema,
        alternarTema,
        toasts,
        adicionarToast,
        removerToast,
      }}
    >
      {children}
    </ContextoSessao.Provider>
  );
}

export function usarSessao() {
  const ctx = useContext(ContextoSessao);
  if (!ctx) {
    throw new Error('usarSessao deve ser usado dentro de ProvedorSessao');
  }
  return ctx;
}
