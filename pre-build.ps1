try {
    npm install --save-dev webpack
    npm install --save-dev webpack-cli
	cd Client
    npm run build
    cd ..
    $commitHash = (git rev-parse HEAD)
    Write-Output $commitHash

    $a = Get-Content 'version.json' -raw | ConvertFrom-Json

    $a.GitCommit = $commitHash

    $a | ConvertTo-Json -depth 32| set-content 'version.json'
}
catch {
	exit 1
}