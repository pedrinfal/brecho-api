import { BrowserRouter, Route, Routes } from 'react-router';
import { Layout } from './componentes/Layout';
import { PaginaAdminPecas } from './paginas/AdminPecas';
import { PaginaCadastro } from './paginas/Cadastro';
import { PaginaDetalhePeca } from './paginas/DetalhePeca';
import { PaginaLogin } from './paginas/Login';
import { PaginaNaoEncontrado } from './paginas/NaoEncontrado';
import { PaginaVendas } from './paginas/Vendas';
import { PaginaVitrine } from './paginas/Vitrine';
import { ProvedorSessao } from './sessao';

export default function App() {
  return (
    <ProvedorSessao>
      <BrowserRouter>
        <Layout>
          <Routes>
            <Route path="/" element={<PaginaVitrine />} />
            <Route path="/login" element={<PaginaLogin />} />
            <Route path="/cadastro" element={<PaginaCadastro />} />
            <Route path="/pecas/:id" element={<PaginaDetalhePeca />} />
            <Route path="/vendas" element={<PaginaVendas />} />
            <Route path="/admin/pecas" element={<PaginaAdminPecas />} />
            <Route path="*" element={<PaginaNaoEncontrado />} />
          </Routes>
        </Layout>
      </BrowserRouter>
    </ProvedorSessao>
  );
}
