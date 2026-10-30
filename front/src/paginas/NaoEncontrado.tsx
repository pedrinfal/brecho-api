import { useEffect } from 'react';
import { Link } from 'react-router';

export function PaginaNaoEncontrado() {
  useEffect(() => {
    document.title = 'Página Não Encontrada · Garimpo Vintage';
  }, []);

  return (
    <div className="estado-vazio">
      <h1>404 · Página não encontrada</h1>
      <p>O endereço que você tentou acessar não existe no Garimpo Vintage.</p>
      <Link to="/" className="botao botao-primario">
        ← Voltar para a vitrine do brechó
      </Link>
    </div>
  );
}
