-- ============================================================
-- 014 - Identity & Security
-- Compatible with current JwellerSaaS database schema
-- ============================================================

-- ============================================================
-- USER
-- ============================================================

create table if not exists "user" (
    user_id bigserial primary key,

    tenant_id bigint not null
        references tenant(tenant_id),

    username varchar(100) not null,
    email varchar(250) not null,
    password_hash text not null,

    is_active boolean not null default true,

    language varchar(20),
    timezone varchar(100),

    created_by bigint,
    created_date timestamptz not null default now(),
    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid(),

    is_system boolean not null default false
);

create unique index if not exists ux_user_tenant_username
    on "user" (tenant_id, upper(username))
    where is_deleted = false;

create unique index if not exists ux_user_tenant_email
    on "user" (tenant_id, upper(email))
    where is_deleted = false;

create index if not exists ix_user_tenant_id
    on "user" (tenant_id);

create index if not exists ix_user_is_active
    on "user" (is_active);

create index if not exists ix_user_is_deleted
    on "user" (is_deleted);


-- ============================================================
-- ROLE
-- ============================================================

create table if not exists role (
    role_id bigserial primary key,

    tenant_id bigint not null
        references tenant(tenant_id),

    role_code varchar(100) not null,
    role_name varchar(200) not null,

    is_system boolean not null default false,

    created_by bigint,
    created_date timestamptz not null default now(),
    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid()
);

create unique index if not exists ux_role_tenant_code
    on role (tenant_id, upper(role_code))
    where is_deleted = false;

create index if not exists ix_role_tenant_id
    on role (tenant_id);

create index if not exists ix_role_is_deleted
    on role (is_deleted);


-- ============================================================
-- PERMISSION
-- ============================================================

create table if not exists permission (
    permission_id bigserial primary key,

    permission_code varchar(150) not null,
    permission_name varchar(200) not null,

    resource varchar(100) not null,
    action varchar(100) not null,

    created_by bigint,
    created_date timestamptz not null default now(),
    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid()
);

create unique index if not exists ux_permission_code
    on permission (upper(permission_code))
    where is_deleted = false;

create index if not exists ix_permission_resource
    on permission (resource);

create index if not exists ix_permission_action
    on permission (action);


-- ============================================================
-- ROLE PERMISSION
-- ============================================================

create table if not exists role_permission (
    role_permission_id bigserial primary key,

    role_id bigint not null
        references role(role_id),

    permission_id bigint not null
        references permission(permission_id),

    created_by bigint,
    created_date timestamptz not null default now(),

    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid()
);

create unique index if not exists ux_role_permission
    on role_permission (role_id, permission_id)
    where is_deleted = false;

create index if not exists ix_role_permission_role_id
    on role_permission (role_id);

create index if not exists ix_role_permission_permission_id
    on role_permission (permission_id);


-- ============================================================
-- USER ROLE
-- ============================================================

create table if not exists user_role (
    user_role_id bigserial primary key,

    user_id bigint not null
        references "user"(user_id),

    role_id bigint not null
        references role(role_id),

    created_by bigint,
    created_date timestamptz not null default now(),

    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid()
);

create unique index if not exists ux_user_role
    on user_role (user_id, role_id)
    where is_deleted = false;

create index if not exists ix_user_role_user_id
    on user_role (user_id);

create index if not exists ix_user_role_role_id
    on user_role (role_id);


-- ============================================================
-- USER BRANCH
-- ============================================================

create table if not exists user_branch (
    user_branch_id bigserial primary key,

    user_id bigint not null
        references "user"(user_id),

    branch_id bigint not null
        references branch(branch_id),

    is_default boolean not null default false,

    created_by bigint,
    created_date timestamptz not null default now(),

    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid()
);

create unique index if not exists ux_user_branch
    on user_branch (user_id, branch_id)
    where is_deleted = false;

create index if not exists ix_user_branch_user_id
    on user_branch (user_id);

create index if not exists ix_user_branch_branch_id
    on user_branch (branch_id);


-- ============================================================
-- REFRESH TOKEN
-- ============================================================

create table if not exists refresh_token (
    refresh_token_id bigserial primary key,

    user_id bigint not null
        references "user"(user_id),

    tenant_id bigint not null
        references tenant(tenant_id),

    token_hash varchar(128) not null,

    expires_at timestamptz not null,
    revoked_at timestamptz,
    replaced_by_token_hash varchar(128),

    created_by bigint,
    created_date timestamptz not null default now(),

    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid()
);

create unique index if not exists ux_refresh_token_hash
    on refresh_token (token_hash);

create index if not exists ix_refresh_token_user_id
    on refresh_token (user_id);

create index if not exists ix_refresh_token_tenant_id
    on refresh_token (tenant_id);

create index if not exists ix_refresh_token_expires_at
    on refresh_token (expires_at);


-- ============================================================
-- PASSWORD HISTORY
-- ============================================================

create table if not exists password_history (
    password_history_id bigserial primary key,

    user_id bigint not null
        references "user"(user_id),

    password_hash text not null,

    created_by bigint,
    created_date timestamptz not null default now(),

    modified_by bigint,
    modified_date timestamptz,

    deleted_by bigint,
    deleted_date timestamptz,

    is_deleted boolean not null default false,

    row_version uuid not null default gen_random_uuid()
);

create index if not exists ix_password_history_user_id
    on password_history (user_id);

create index if not exists ix_password_history_created_date
    on password_history (user_id, created_date);
