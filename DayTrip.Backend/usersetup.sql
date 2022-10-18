drop table users cascade;
create table if not exists users
(
    userid       uuid         not null
        constraint pk_users
            primary key,
    friendlyname varchar(255) not null,
    email        varchar(255) not null
        constraint pk_email
            unique,
    created      timestamp    not null,
    organization uuid
);

alter table users
    owner to localpin;

drop table credentials cascade;
create table if not exists credentials
(
    credentialid uuid          not null
        constraint pk_credentials
            primary key,
    userid       uuid          not null
        constraint fk_12
            references users,
    kind         integer       not null,
    identifier   varchar(1023) not null,
    secret       varchar(1023) not null
);

alter table credentials
    owner to localpin;

drop table pin_types cascade;
create table if not exists pin_types
(
    id    uuid         not null
        constraint pin_types_pk
            primary key,
    name  varchar(127) not null,
    icon  varchar(1023),
    color varchar(63)  not null
);

alter table pin_types
    owner to localpin;

create unique index pin_types_id_uindex
    on pin_types (id);

drop table pins cascade;
create table if not exists pins
(
    id      uuid             not null
        constraint pins_pk
            primary key,
    author  uuid             not null,
    title   varchar(255)     not null,
    lat     double precision not null,
    lon     double precision not null,
    kind    integer          not null,
    kind_id uuid
        constraint pins_pin_types_id_fk
            references pin_types,
    expires timestamp,
    status  integer          not null,
    created timestamp        not null
);

alter table pins
    owner to localpin;

create unique index pins_id_uindex
    on pins (id);

drop table pin_bodies cascade;
create table if not exists pin_bodies
(
    id          uuid not null
        constraint pin_bodies_pk
            primary key,
    image       varchar(1023),
    description text
);

alter table pin_bodies
    owner to localpin;

create unique index pin_bodies_id_uindex
    on pin_bodies (id);

drop table comments cascade;
create table if not exists comments
(
    id      uuid      not null
        constraint comments_pk
            primary key,
    author  uuid      not null
        constraint comments_users_userid_fk
            references users,
    pin     uuid      not null
        constraint comments_pins_id_fk
            references pins,
    created timestamp not null,
    text    text      not null
);

alter table comments
    owner to localpin;

create unique index comments_id_uindex
    on comments (id);