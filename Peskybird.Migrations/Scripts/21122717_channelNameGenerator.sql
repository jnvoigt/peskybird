BEGIN
    TRANSACTION;


alter table ChannelConfigs
    add name_generator int NOT NULL;

commit TRANSACTION;