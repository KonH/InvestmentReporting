<template>
	<div>...</div>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';

export default class App extends Vue {
	async mounted() {
		const isLoggedIn = await this.checkLogin();
		console.log('Is logged in? ' + isLoggedIn);
		if (!isLoggedIn) {
			console.log('Perform register/login routine');
			await this.testRegister();
			await this.testLogin();
			await this.checkLogin();
		}
		console.log('Access authorized features');
		await this.testHello();
	}

	async testRegister() {
		const result = await fetch(
			'api/auth/v1/register?userName=testUser&password=TestPassword%401',
			{
				method: 'POST',
			}
		);
		console.log(result.url);
		const status = result.status;
		const statusText = result.statusText;
		const text = result.ok ? await result.text() : '';
		alert(`Register: ${status} ${statusText} ${text}`);
	}

	async checkLogin() {
		const result = await fetch('api/auth/v1/check');
		console.log(result.url);
		const status = result.status;
		const statusText = result.statusText;
		const text = result.ok ? await result.text() : '';
		alert(`Check: ${status} ${statusText} ${text}`);
		return result.ok;
	}

	async testLogin() {
		const result = await fetch(
			'api/auth/v1/login?userName=testUser&password=TestPassword%401',
			{
				method: 'POST',
			}
		);
		console.log(result.url);
		const status = result.status;
		const statusText = result.statusText;
		const text = result.ok ? await result.text() : '';
		alert(`Login: ${status} ${statusText} ${text}`);
	}

	async testHello() {
		const result = await fetch('api/test/v1/hello');
		console.log(result.url);
		const status = result.status;
		const statusText = result.statusText;
		const text = result.ok ? await result.text() : '';
		alert(`Hello: ${status} ${statusText} ${text}`);
	}
}
</script>
