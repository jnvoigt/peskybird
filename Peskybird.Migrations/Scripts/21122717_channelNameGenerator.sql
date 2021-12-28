BEGIN
    TRANSACTION;


alter table ChannelConfigs
    add name_generator int default 0 NOT NULL;

commit TRANSACTION;