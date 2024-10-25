-- MIGRONDI:NAME=AddInitialTables_1729800400460.sql
-- MIGRONDI:TIMESTAMP=1729800400460
-- ---------- MIGRONDI:UP ----------
-- Add your SQL migration code below. You can delete this line but do not delete the comments above.

create table authors(
    id uuid default gen_random_uuid() primary key,
    name text not null,
    email text not null,
    bio text not null,
    social_networks jsonb
);
create unique index authors_email_unique on authors(email);

create type post_status as enum ('draft', 'published');

create table posts(
    id uuid default gen_random_uuid() primary key,
    title text not null,
    content text not null,
    status post_status not null,
    slug text,
    author_id uuid,
    created_at timestamptz not null,
    updated_at timestamptz not null,
    published_at timestamptz,
    foreign key (author_id) references authors(id)
);
create index title_idx on posts(title);
create index status_idx on posts(status);
create index content on posts using gin(to_tsvector('english', content));

-- ---------- MIGRONDI:DOWN ----------
-- Add your SQL rollback code below. You can delete this line but do not delete the comment above.
do $$
    begin
    raise exception 'This migration cannot be rolled back';
end $$;
