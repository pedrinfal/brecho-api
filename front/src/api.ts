export class ErroApi extends Error {
  status: number;
  errosValidacao: string[];

  constructor(status: number, mensagem: string, errosValidacao: string[] = []) {
    super(mensagem);
    this.name = 'ErroApi';
    this.status = status;
    this.errosValidacao = errosValidacao;
  }
}

export interface Peca {
  id: number;
  nome: string;
  descricao: string;
  tamanho: string;
  categoria: string;
  condicao: string;
  preco: number;
  vendida: boolean;
  ativa: boolean;
  criadaEm: string;
}

export interface ListaPaginada<T> {
  itens: T[];
  pagina: number;
  tamanho: number;
  total: number;
  totalPaginas: number;
}

export interface Venda {
  id: number;
  pecaId: number;
  nomePeca: string;
  tamanhoPeca: string;
  precoPeca: number;
  usuarioId: number;
  nomeCliente: string;
  dataVenda: string;
  formaPagamento: string;
  observacoes?: string | null;
  situacao: string;
}

export interface AuthResposta {
  id: number;
  nome: string;
  email: string;
  papel: 'admin' | 'cliente';
  token: string;
  expiraEm: string;
}

const BASE_URL = (import.meta.env.VITE_API_URL || 'http://localhost:5187').replace(/\/+$/, '');

let manipuladorNaoAutorizado: (() => void) | null = null;

export function registrarManipulador401(fn: () => void) {
  manipuladorNaoAutorizado = fn;
}

export async function requisicaoApi<T>(
  caminho: string,
  opcoes: RequestInit = {},
  token?: string | null
): Promise<T> {
  const cabecalhos = new Headers(opcoes.headers || {});
  if (opcoes.body && !cabecalhos.has('Content-Type')) {
    cabecalhos.set('Content-Type', 'application/json');
  }
  if (token) {
    cabecalhos.set('Authorization', `Bearer ${token}`);
  }

  let resposta: Response;
  try {
    resposta = await fetch(`${BASE_URL}${caminho}`, {
      ...opcoes,
      headers: cabecalhos,
    });
  } catch {
    throw new ErroApi(
      0,
      'Não foi possível falar com a API. Verifique sua conexão ou aguarde alguns segundos se o servidor gratuito estiver acordando.'
    );
  }

  if (resposta.status === 401 && token && manipuladorNaoAutorizado) {
    manipuladorNaoAutorizado();
  }

  if (!resposta.ok) {
    let mensagem = `Erro HTTP ${resposta.status}`;
    const listaErros: string[] = [];

    try {
      const corpo = await resposta.json();
      if (corpo && typeof corpo === 'object') {
        if (corpo.errors && typeof corpo.errors === 'object') {
          for (const chave of Object.keys(corpo.errors)) {
            const msgs = corpo.errors[chave];
            if (Array.isArray(msgs)) {
              for (const m of msgs) {
                listaErros.push(String(m));
              }
            }
          }
        }
        if (typeof corpo.detail === 'string' && corpo.detail.trim() !== '') {
          mensagem = corpo.detail;
        } else if (listaErros.length > 0) {
          mensagem = listaErros.join(' ');
        } else if (typeof corpo.title === 'string' && corpo.title.trim() !== '') {
          mensagem = corpo.title;
        }
      }
    } catch {
      if (resposta.status === 401) {
        mensagem = 'Sua sessão expirou ou você precisa entrar para continuar.';
      } else if (resposta.status === 403) {
        mensagem = 'Você não tem permissão para realizar esta operação.';
      } else if (resposta.status === 404) {
        mensagem = 'O recurso solicitado não foi encontrado.';
      }
    }

    throw new ErroApi(resposta.status, mensagem, listaErros);
  }

  if (resposta.status === 204) {
    return undefined as T;
  }

  return (await resposta.json()) as T;
}
