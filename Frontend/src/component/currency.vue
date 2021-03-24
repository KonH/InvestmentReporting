<template>
	<b>{{ displayName }}</b>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';
import { CurrencyDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

export default class Currency extends Vue {
	@Prop()
	currency: CurrencyDto | undefined;

	get displayName() {
		return this.currency?.code;
	}
}
</script>
