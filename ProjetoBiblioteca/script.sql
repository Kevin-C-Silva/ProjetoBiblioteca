drop database if exists bdBiblioteca;
create database bdBiblioteca;
use bdBiblioteca;

create table Usuarios(
	id int primary key auto_increment,
	nome varchar(100),
	email varchar(100),
	senha_Hash varchar(255),
	role enum ("Bibliotecario","Admin"),
	ativo tinyint(1) default 1,
	criado_em datetime default current_timestamp
);

delimiter $$
drop procedure if exists sp_usuario_criar $$
create procedure sp_usuario_criar (
    in p_nome varchar(100),
    in p_email varchar(100),
    in p_senha_hash varchar(255),
    in p_role varchar(20) 
)
begin
    insert into Usuarios (nome, email, senha_Hash, role, ativo, criado_em)
    values (p_nome, p_email, p_senha_hash, p_role, 1, NOW());
end $$

call sp_usuario_criar(
    'João Admin',
    'joao@biblioteca.com',
    '$2a$11$HASHADMINEXEMPLO9876543210',
    'Admin'
);

create table Editoras(
	id int primary key auto_increment,
    nome varchar(150) not null,
    criado_em datetime not null default current_timestamp
);

create table Generos(
	id int primary key auto_increment,
    nome varchar(100) not null,
    criado_em datetime not null default current_timestamp
);

create table Autores(
	id int primary key auto_increment,
    nome varchar(150) not null,
    criado_em datetime not null default current_timestamp
);

create table Livros(
	id int primary key auto_increment,
    titulo varchar(200) not null,
    autorId int,
    editoraId int,
    generoId int,
    ano smallint,
    isbn varchar(32),
    quantidade_total int,
    quantidade_disponivel int,
    criado_em datetime not null default current_timestamp
);
select * from Livros;

alter table Livros add constraint fk_livros_autor foreign key (autorId) references Autores(id),
				   add constraint fk_livros_editora foreign key (editoraId) references Editoras(id),
                   add constraint fk_livros_genero foreign key (generoId) references Generos(id);

delimiter $$
drop procedure if exists sp_editora_criar $$
create procedure sp_editora_criar(in p_nome varchar(150))
begin
	insert into Editoras (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_genero_criar $$
create procedure sp_genero_criar(in p_nome varchar(100))
begin
	insert into Generos (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_autor_criar $$
create procedure sp_autor_criar(in p_nome varchar(150))
begin
	insert into Autores (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_autor_listar $$
create procedure sp_autor_listar()
begin
	select id, nome from Autores order by nome;
end; $$

delimiter $$
drop procedure if exists sp_editora_listar $$
create procedure sp_editora_listar()
begin
	select id, nome from Editoras order by nome;
end; $$

delimiter $$
drop procedure if exists sp_genero_listar $$
create procedure sp_genero_listar()
begin
	select id, nome from Generos order by nome;
end; $$

delimiter $$
drop procedure if exists sp_livro_criar $$
create procedure sp_livro_criar (
	in p_titulo varchar(200),
    in p_autor int,
    in p_editora int,
    in p_genero int,
    in p_ano smallint,
    in p_isbn varchar(32),
    in p_capa_arquivo varchar(255),
    in p_quantidade int)
begin
	insert into Livros(titulo, autorId, editoraId, generoId, ano, isbn, capa_arquivo, quantidade_total, quantidade_disponivel)
				values(p_titulo, p_autor, p_editora, p_genero, p_ano, p_isbn, p_capa_arquivo, p_quantidade, p_quantidade);
end; $$

delimiter $$
drop procedure if exists sp_livro_listar $$
create procedure sp_livro_listar ()
begin
	select
		l.id,
        l.titulo,
        l.autorId,
        a.nome as autor_nome,
        l.editoraId,
        e.nome as editora_nome,
        l.generoId,
        g.nome as genero_nome,
        l.ano,
        l.isbn,
        l.capa_arquivo,
        l.quantidade_total,
        l.quantidade_disponivel,
        l.criado_em
	from Livros l
    left join Autores a on a.id = l.autorId
    left join Editoras e on e.id = l.editoraId
    left join Generos g on g.id = l.generoId
    order by l.titulo;
end; $$

delimiter $$
drop procedure if exists sp_usuario_obter_por_email $$
create procedure sp_usuario_obter_por_email(in p_email varchar(100))
begin
	select id, nome, email, senha_hash, role, ativo from Usuarios where email = p_email limit 1;
end $$

delimiter $$
drop procedure if exists sp_editora_editar $$
create procedure sp_editora_editar(p_id int, in p_nome varchar(150))
begin
	update Editoras set nome = p_nome where id = p_id;
end;
$$

delimiter $$
drop procedure if exists sp_genero_editar $$
create procedure sp_genero_editar(in p_nome varchar(100))
begin
	insert into Generos (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_autor_editar $$
create procedure sp_autor_editar(in p_nome varchar(150))
begin
	insert into Autores (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_livro_obter $$
create procedure sp_livro_obter (in p_id int)
begin
	select id, titulo, autorId, editoraId, generoId, ano, isbn, quantidade_total, quantidade_disponivel, criado_em from Livros where id = p_id;
end $$

delimiter $$
drop procedure if exists sp_livro_atualizar $$
create procedure sp_livro_atualizar (in p_id int, in p_titulo varchar(200), in p_autor int, in p_editora int, in p_genero int, in p_ano smallint, in p_isbn varchar(32), in p_novo_total int)
begin
	declare v_disp int;
    declare v_total int;
    
    select quantidade_disponivel, quantidade_total into v_disp, v_total
    from Livros where id = p_id for update;
    
    update Livros
		set titulo = p_titulo, autor = p_autor, editora = p_editora, genero = p_genero, ano = p_ano, isbn = p_isbn, quantidade_total = p_novo_total, 
        quantidade_disponivel = greatest(0, least(p_novo_total, v_disp + (p_novo_total - v_total))) where id = p_id;
end $$

delimiter $$
drop procedure if exists sp_livro_excluir $$
create procedure sp_livro_excluir (in p_id int)
begin
	delete from Livros where id = p_id;
end $$

alter table Livros add column capa_arquivo varchar(255) null after isbn;

create table Bibliotecarios(
	id int primary key,
    matricula varchar(30) unique null,
    criado_em datetime null default current_timestamp
);

create table Leitor(
	id int primary key auto_increment,
    id_emprestimo int not null,
    id_livro int not null,
    quantidade int not null default 1,
    data_devolucao_item datetime null
);

create table Emprestimos(
	id int primary key auto_increment,
    id_leitor int not null,
    id_bibliotecario int not null,
    data_emprestimo datetime not null default current_timestamp,
    data_prevista_devolucao date not null,
    data_devolucao_geral datetime not null,
    status ENUM('Ativo','Finalizado','Parcial') not null default 'Ativo'
);

create table Emprestimo_itens(
	id int primary key auto_increment,
    id_emprestimo int not null,
    id_livro int not null,
    quantidade int not null default 1,
    data_devolucao_item datetime null
);

alter table Emprestimo_itens
	add constraint fk_itens_emp foreign key (id_emprestimo) references Emprestimos(id),
    add constraint fk_itens_livro foreign key (id_livro) references Livros(id),