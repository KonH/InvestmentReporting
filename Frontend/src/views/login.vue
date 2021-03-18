<template>
	<h1>Login</h1>
	<label>
		Username:
		<input ref="login" type="text" />
	</label>
	<label>
		Password:
		<input ref="password" type="password" />
	</label>
	<button :onclick="onclick">Login</button>
	<router-link to="/register">Register</router-link>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';

export default class Login extends Vue {
	loginInput: HTMLInputElement | undefined;
	passwordInput: HTMLInputElement | undefined;

	mounted() {
		this.loginInput = this.$refs.login as HTMLInputElement;
		this.passwordInput = this.$refs.password as HTMLInputElement;
	}

	async onclick() {
		if (!this.loginInput || !this.passwordInput) {
			console.error(
				`invalid setup (loginInput: ${this.loginInput}, passwordInput: ${this.passwordInput})`
			);
			return;
		}
		const username = encodeURIComponent(this.loginInput.value);
		const password = encodeURIComponent(this.passwordInput.value);
		const loginUrl = `api/auth/v1/login?userName=${username}&password=${password}`;
		const loginResult = await Backend.post(loginUrl);
		if (loginResult.ok) {
			await router.push('/');
		} else {
			alert('Login failed');
		}
	}
}
</script>
