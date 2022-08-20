# DaprTest

to run the project using dapr, please run the below command

dapr run --app-id testdapr --app-protocol grpc --app-port 8013 dotnet run



to run it using postman use the below url

curl --location --request POST 'http://localhost:{{dapr-port}}/v1.0/publish/pubsub/DataChanged' \
--header 'Content-Type: application/json' \
--data-raw '{
    "info": "Test",
}'


-to get dapr port use the below command

dapr dashboard

-select testdapr
-copy the port beside Dapr HTTP Port and put it in the postman url