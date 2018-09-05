﻿/******
EVENT STORE
******/

CREATE TABLE IF NOT EXISTS RES_EVENT_STREAM (
	RES_EVS_EventStreamId uuid NOT NULL PRIMARY KEY,
	RES_EVS_Version int NOT NULL,
	RES_EVS_MetadataJson text
);

CREATE TABLE IF NOT EXISTS RES_EVENT_STREAM_ROW (
	RES_ESR_EventStreamRowId uuid NOT NULL PRIMARY KEY,
	RES_ESR_GlobalSequenceNumber bigint not null GENERATED ALWAYS AS IDENTITY UNIQUE,
	RES_ESR_StreamId uuid NOT NULL REFERENCES RES_EVENT_STREAM,
	RES_ESR_StreamSequenceNumber bigint NOT NULL,
	RES_ESR_StoreDate timestamptz NOT NULL,
	RES_ESR_EventName text NOT NULL,
	RES_ESR_EventVersion int NOT NULL,
	RES_ESR_EventJson text NOT NULL,
	RES_ESR_AdditionalMetadataJson text,
	RES_ESR_IsDispatchedToAsyncQueues boolean NOT NULL,
	UNIQUE(RES_ESR_StreamId, RES_ESR_StreamSequenceNumber)
);

/******
ASYNC EVENTS
******/

CREATE TABLE IF NOT EXISTS RAE_ASYNC_EVENT_QUEUE (
	RAE_AEQ_AsyncEventQueueId text NOT NULL PRIMARY KEY,
	RAE_AEQ_Version int NOT NULL,
	RAE_AEQ_LastSequenceNumberProcessed bigint
);

CREATE TABLE IF NOT EXISTS RAE_EXTERNAL_EVENT_RECORD (
	RAE_EER_ExternalEventRecordId uuid NOT NULL PRIMARY KEY,
	RAE_EER_EventName text NOT NULL,
	RAE_EER_EventVersion int NOT NULL,
	RAE_EER_EventJson text NOT NULL,
	RAE_EER_MetadataJson text
);

CREATE TABLE IF NOT EXISTS RAE_QUEUED_ASYNC_EVENT (
	RAE_QAE_QueuedAsyncEventId uuid NOT NULL PRIMARY KEY,
	RAE_QAE_QueueId text NOT NULL REFERENCES RAE_ASYNC_EVENT_QUEUE,
	RAE_QAE_SequenceNumber bigint,
	RAE_QAE_EventStreamRowId uuid REFERENCES RES_EVENT_STREAM_ROW,
	RAE_QAE_ExternalEventRecordId uuid REFERENCES RAE_EXTERNAL_EVENT_RECORD,
	UNIQUE(RAE_QAE_QueueId, RAE_QAE_SequenceNumber)
);

/******
SAGAS
******/

CREATE TABLE IF NOT EXISTS REV_SAGA_METADATA_RECORD (
	REV_SMR_SagaMetadataRecordId uuid NOT NULL PRIMARY KEY,
	REV_SMR_ClassId uuid NOT NULL
);

CREATE TABLE IF NOT EXISTS REV_SAGA_METADATA_KEY (
	REV_SMK_SagaMetadataKeyId uuid NOT NULL PRIMARY KEY,
	REV_SMK_SagaId uuid NOT NULL REFERENCES REV_SAGA_METADATA_RECORD,
	REV_SMK_KeyName VARCHAR (128) NOT NULL,
	REV_SMK_KeyValue text NOT NULL
);
